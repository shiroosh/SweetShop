﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using final_project.Models;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace final_project.Controllers
{
    public class CheckoutController : Controller
    {
        private const string BaseCurrencyApiURL = "https://api.exchangeratesapi.io/latest?base=ILS&symbols=";

        public Currency CurrentCurrency { get; set; }

        public double CurrencyExchangeRate { get; set; }

        public List<Product> Cart { get; set; } = new List<Product>()
        {
            new Product()
                {ID = 1, Name = "chocolate cake", Price = 12, Category = new Category() {ID = 1, Name = "cakes"}},
            new Product() {ID = 1, Name = "cheese cake", Price = 12, Category = new Category() {ID = 1, Name = "cakes"}}
        }; //TODO: get cart from session

        // GET: /<controller>/{choosenCurrency}
        public async Task<IActionResult> Checkout(List<string> invalidFieldsList, Currency choosenCurrency = Currency.ILS)
        {
            await UpdateCurrency(choosenCurrency);
            ViewBag.CurrentCurrency = CurrentCurrency;
            ViewBag.Cart = Cart;
            ViewBag.ConvertToCurrentCurrency = new Func<double, double>(ConvertToCurrentCurrency);
            ViewBag.CartSum = ConvertToCurrentCurrency(GetCartSum());
            ViewBag.CartSize = Cart.Count;
            ViewBag.invalidFieldsList = invalidFieldsList;
            ViewBag.GetInputClass = new Func<string, string>(GetInputClass);
            return View();
        }

        [HttpPost]
        public IActionResult AddOrder(Order order)
        {
            var invalidFields = new List<string>();

            if (order.FirstName == null) invalidFields.Add("FirstName");

            if (order.LastName == null) invalidFields.Add("LastName");

            if (order.Email == null || !(new EmailAddressAttribute().IsValid(order.Email))) invalidFields.Add("Email");

            if (order.Address == null) invalidFields.Add("Address");

            if (order.Zip == null || order.Zip?.Length != 7) invalidFields.Add("Zip");

            if (order.CCName == null) invalidFields.Add("CCName");

            if (order.CCNumber == null || order.CCNumber?.Length != 16) invalidFields.Add("CCNumber");

            if (order.CCExpiration == null || !Regex.IsMatch(order.CCExpiration, @"([0][1-9]|[1][0-2])/\d{2}")) invalidFields.Add("CCExpiration");

            if (order.CCCvv == null || order.CCCvv?.Length != 3) invalidFields.Add("CCCvv");

            if (invalidFields.Count != 0) return RedirectToAction("Checkout", new { invalidFieldsList = invalidFields, choosenCurrency = CurrentCurrency});

            // Add to DB

            return View("OrderComplete");
        }

        public double GetCartSum()
        {
            return Cart.Sum(x => x.Price);
        }

        public double ConvertToCurrentCurrency(double value)
        {
            return value * CurrencyExchangeRate;
        }

        public async Task<double> GetCurrencyExchangeRate(Currency wantedCurrency)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{BaseCurrencyApiURL}{CurrentCurrency}");
            response.EnsureSuccessStatusCode();

            var returnVal = JObject.Parse(await response.Content.ReadAsStringAsync());

            return (double) returnVal["rates"][wantedCurrency.ToString()];
        }

        public async Task UpdateCurrency(Currency newCurrency)
        {
            CurrentCurrency = newCurrency;
            CurrencyExchangeRate = await GetCurrencyExchangeRate(newCurrency);
        }

        public string GetInputClass(string inputName)
        {
            var invalidFieldsList = (List<string>)ViewBag.invalidFieldsList;

            if (invalidFieldsList.Count == 0) return "form-control";

            return invalidFieldsList.Contains(inputName) ? "form-control is-invalid" : "form-control is-valid";
        }



    }
}