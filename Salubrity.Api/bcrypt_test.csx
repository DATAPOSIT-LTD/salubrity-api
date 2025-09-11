#r "nuget: BCrypt.Net-Next, 4.0.2"

using System;
using BCrypt.Net;

var password = "string"; // Try "string", "string ", etc.
var hash = "$2a$12$vu8XAZvbXY2W5n1NZfiSFO8EUswa3obbP43Lq/O0fsFUFkV1yMKDC";

Console.WriteLine($"Password Valid: {BCrypt.Net.BCrypt.Verify(password, hash)}");
