using E_Book_eproject.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Security.Claims;

namespace E_Book_eproject.Controllers
{
    public class CartController : Controller
    {
        private static Random _random = new Random();
        EProjectContext db = new EProjectContext();
        [HttpPost]
        [Authorize (Roles ="User")]
        public IActionResult AddToCart(Cart cart, int qty)
        {
            var userId = User.FindFirstValue(ClaimTypes.Sid); 

            if (userId != null)
            {
                cart.UserId = Convert.ToInt32(userId);
                cart.Price = qty;

                var productdetails= db.Products.FirstOrDefault(x => x.Id== cart.ProductId);
                cart.Price= productdetails?.Price;
                cart.Quantity= qty;
             
                db.Carts.Add(cart);
                db.SaveChanges();

                return RedirectToAction("MyCart");
            }
            else
            {
                return RedirectToAction("Login", "Auth"); 
            }

        }
                  
        public IActionResult Delete(int id)
        {
            var cart = db.Carts.Find(id);

            if (cart == null)
            {
                return NotFound();  // or handle it based on your application's logic
            }

            db.Carts.Remove(cart);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Product successfully Remove!";

            return RedirectToAction("MyCart"); // It's better to redirect instead of returning a view
        }


        public IActionResult MyCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.Sid);

            if (userId != null)
            {
                int UserId = Convert.ToInt32(userId);

                // Joining Cart with Product using ProductId
                var cartItems = (from cart in db.Carts
                                 join product in db.Products
                                 on cart.ProductId equals product.Id
                                 where cart.UserId == UserId
                                 select new CheckoutViewModel
                                 {
                                     Id = cart.Id,
                                     Quantity = cart.Quantity,
                                     Name = product.Name,
                                     Price = (int)product.Price,
                                     Image = product.Image
                                 });

                // Pass the data to the view
                return View(cartItems.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }

        }
        [Authorize (Roles ="User")]
        public IActionResult checkout()
        {
            
            return View();
        }




        // product details check out 
        [HttpPost]
        public IActionResult checkout(Order ord)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Sid);
            if (userIdString != null)
            {
                int userId = Convert.ToInt32(userIdString);

                // Fetch user's cart data
                var cartItems = db.Carts.Where(x => x.UserId == userId).ToList();

                // Generate random order number
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var orderNumber = new string(Enumerable.Repeat(chars, 8)
                                              .Select(s => s[_random.Next(s.Length)]).ToArray());

                // Calculate delivery charges based on distance
                int deliveryCharges = (ord.DeliveryDistance < 3) ? 0 : (int)(ord.DeliveryDistance * 70);

                // Initialize total amount with delivery charges
                decimal totalAmount = deliveryCharges;

                // Calculate total amount from cart items
                foreach (var item in cartItems)
                {
                    var itemPrice = item.Price ?? 0; // Handle nullable Price
                    var itemQuantity = item.Quantity ; // Handle nullable Quantity

                    totalAmount += itemPrice * itemQuantity;
                }

                // Create and save the order
                Order newOrder = new Order()
                {
                    OrderNumber = orderNumber,
                    TotalAmount = totalAmount,
                    DeliveryDistance = ord.DeliveryDistance,
                    CustomerId = userId,
                };

                var addOrder = db.Orders.Add(newOrder);
                db.SaveChanges();

                // Add order details for each cart item
                foreach (var item in cartItems)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderId = addOrder.Entity.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity , // Handle nullable Quantity
                        UnitPrice = item.Price ?? 0, // Handle nullable Price
                    };

                    db.OrderDetails.Add(orderDetail);
                    db.Carts.Remove(item); // Remove cart item after saving the order details
                }

                // Save all changes at once
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }



        //public IActionResult checkout(Order ord)
        //    {
        //        var UserId =User.FindFirstValue(ClaimTypes.Sid);
        //        if (UserId != null) { 

        //            int userId = Convert.ToInt32(UserId);
        //        var data = db.Carts.Where(x=>x.UserId==userId).ToList();
        //        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        //        var ordernumber = new string(Enumerable.Repeat(chars, 8)
        //          .Select(s => s[_random.Next(s.Length)]).ToArray());

        //        int deliveryCharges = 0;
        //        if(ord.DeliveryDistance < 3)
        //        {
        //            deliveryCharges = 0;

        //        }
        //        else
        //        {
        //            deliveryCharges = (int)(ord.DeliveryDistance * 70);
        //        }
        //        decimal totalAmount = deliveryCharges;
        //        foreach (var item in data)
        //        {
        //            totalAmount = (decimal)(totalAmount + (item.Price * item.Quantity));




        //        }


        //        Order order=new Order()
        //        {
        //           OrderNumber=ordernumber,
        //           TotalAmount=totalAmount,
        //           DeliveryDistance=ord.DeliveryDistance,
        //           CustomerId=userId,

        //        };

        //        var addOrder= db.Orders.Add(order); 
        //        db.SaveChanges();


        //        foreach (var item in data)
        //        {
        //            OrderDetail orderDetails = new OrderDetail()
        //            {
        //                OrderId=addOrder.Entity.OrderId,
        //                ProductId=item.ProductId,
        //                Quantity=item.Quantity,
        //                UnitPrice= (decimal)item.Price,

        //            };

        //            db.OrderDetails.Add(orderDetails);
        //            db.Carts.Remove(item);
        //            db.SaveChanges();



        //        }


        //    }
        //        return RedirectToAction("Index", "Home");
        //    }


    }
}
