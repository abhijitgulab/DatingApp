using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _content;
        public DatingRepository(DataContext content)
        {
            this._content = content;

        }
        public void Add<T>(T entity) where T : class
        {
            _content.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _content.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
           var user = await _content.Users.Include(p => p.Photos).FirstOrDefaultAsync(u=> u.Id == id);
           return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _content.Users.Include(p => p.Photos).ToListAsync();
            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await _content.SaveChangesAsync() > 0;
        }
    }
}