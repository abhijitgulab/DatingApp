using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
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

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _content.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _content.Photos.Where(p => p.UserId == userId).FirstOrDefaultAsync(p => p.IsMain == true);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _content.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messsages = _content.Messages
                            .Include(u => u.Sender).ThenInclude(p => p.Photos)
                            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                            .AsQueryable();

            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                                messsages = messsages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                                break;
                case "Outbox":
                                messsages = messsages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                                break;
                default:
                                messsages = messsages.Where(u => u.RecipientId == messageParams.UserId && u.IsRead == false && u.RecipientDeleted == false);
                                break;
            }

            messsages = messsages.OrderByDescending(d => d.MessageSent);

            return PagedList<Message>.CreateAsync(messsages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messsages = await _content.Messages
                            .Include(u => u.Sender).ThenInclude(p => p.Photos)
                            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                            .Where(m => m.RecipientId == userId && m.SenderId == recipientId && m.RecipientDeleted == false ||
                            m.SenderId == userId && m.RecipientId == recipientId && m.SenderDeleted == false)
                            .OrderByDescending(m => m.MessageSent)
                            .ToListAsync();

                return messsages;                          
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _content.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;

        }

        public async Task<User> GetUser(int id)
        {
           var user = await _content.Users.Include(p => p.Photos).FirstOrDefaultAsync(u=> u.Id == id);
           return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _content.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);
            if(userParams.MinAge != 18 || userParams.MaxAge !=99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u=> u.DateOfBirth >= minDob && u.DateOfBirth < maxDob);
            }

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));

            }

            if(userParams.Likees)
            {

                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));

            }
            
            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
               switch (userParams.OrderBy)
               {
                   case "created":
                   users = users.OrderByDescending(u => u.Created);
                   break;
                   default:
                   users = users.OrderByDescending(u => u.LastActive);
                   break;
               }
            }

            


            return await PagedList<User>.CreateAsync(users,userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _content.SaveChangesAsync() > 0;
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _content.Users.Include(x => x.Likers).Include(x => x.Likees).FirstOrDefaultAsync(x => x.Id == id);

            if(likers)
            {
                return  user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return  user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }

        }
    }
}