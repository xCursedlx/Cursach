using ProductManage.Data;
using ProductManage.Models;
using System.Threading.Tasks;
namespace ProductManage.Services
{
    public class AuthService
    {
        private static UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        public static User CurrentUser { get; private set; }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        public static async Task<User> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.AuthenticateAsync(username, password);
            if (user != null && user.IsActive)
            {
                CurrentUser = user;
            }
            return user;
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}