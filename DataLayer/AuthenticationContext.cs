using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer;

namespace DataLayer
{
    public class AuthenticationContext
    {
        private readonly UserContext _userContext;

        public AuthenticationContext(UserContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }
        private string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        public User Authenticate(string email, string password)
        {
            // Simulate database lookup
            // In a real application, you would query the database to get the user's hashed password
            var user = _userContext.GetByEmail(email);
            if (user == null)
            {
                return null; // User not found
            }
            if (VerifyPassword(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public bool Register(User user)
        {
            // Check if user already exists
            var existingUser = _userContext.GetByEmail(user.Email);
            if (existingUser != null)
            {
                return false; // User already exists
            }
            // Hash the password before storing
            user.Password = HashPassword(user.Password);
            // Create the user in the database
            return _userContext.Create(user);
        }
    }
}
