﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HesaUser.Lookup.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace HesaUser.Lookup.Controllers
{
    public class UsersController : Controller
    {
        private readonly HesaUserAPISettings _HesaUserAPISettings;

        public UsersController(IOptions<HesaUserAPISettings> options)
        {
            _HesaUserAPISettings = options.Value;
        }

        [Route("/Users")]
        [HttpGet("{filter}")]
        public IActionResult Users(string filter)
        {
            Users users = new Users();

            if (string.IsNullOrEmpty(filter))
            {
                users.UserList = new List<User>();
                return View(users);
            }

            Regex r = new Regex("^[a-z ,.'-]+$", RegexOptions.IgnoreCase);
            Match m = r.Match(filter);
            if (!m.Success)
            {
                ModelState.AddModelError("", "Unexpected Surname entered");
                users.UserList = new List<User>();
                return View(users);
            }

            string URI = _HesaUserAPISettings.URI + "/Users/" + filter;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URI);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();

                        users.UserList = JsonConvert.DeserializeObject <List<User>> (json);
                    }
                }
            }

            return View(users);
        }
    }
}