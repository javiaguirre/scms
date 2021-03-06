﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCMS.Models;
using SCMS.Models.Interface;
using SCMS.Datas.DBContext;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;
using SCMS.Models.ViewModels;
using System.IO;

namespace SCMS.Datas
{
    public class SCMSRepositoryEF : ISCMS
    {
        SCMSDBContext _ctx = new SCMSDBContext();


        #region "Other"
        public Response ReturnSuccess()
        {
            return new Response() { Success = true, ErrorMessage = "" };
        }

        public byte[] ConvertImgToByte(HttpPostedFileBase file)
        {
            byte[] imageByte = null;
            BinaryReader rdr = new BinaryReader(file.InputStream);
            imageByte = rdr.ReadBytes((int)file.ContentLength);
            return imageByte;
        }

        public bool Login(string userName, string password)
        {
            var userMgr = HttpContext.Current.GetOwinContext().GetUserManager<UserManager<User>>();
            var user = userMgr.Find(userName, password);
            if (user != null && user.IsActive == true)
            {
                var identity = userMgr.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                var authManager = HttpContext.Current.GetOwinContext().Authentication;
                authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
                CurrentUser.User = user;
                return true;
            }
            return false;
        }

        public bool Logout()
        {
            var ctx = HttpContext.Current.GetOwinContext();
            var authMgr = ctx.Authentication;
            authMgr.SignOut("ApplicationCookie");

            return true;
        }

        #endregion

        #region "Info"
        public List<Info> GetInfoList()
        {
            return _ctx.Infos.ToList();
        }

        public int AddInfo(Info info)
        {
            _ctx.Infos.Add(info);  
            _ctx.SaveChanges(); 
            return _ctx.Infos.Max(i => i.InfoId);
         }

        public bool UpdateInfo(Info info)
        {
            _ctx.Entry(info).State = System.Data.Entity.EntityState.Modified;
            _ctx.SaveChanges();
            return true;
        }

        public bool DeleteInfo(int InfoId)
        {
            Info info = GetInfoById(InfoId);
            if(info != null)
            {
                _ctx.Entry(info).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public List<Info> GetCurrentInfo()
        {
            return GetInfoList().Where(i => i.FDate <= DateTime.Now.Date && i.TDate >= DateTime.Now.Date).ToList();
        }

        public List<Info> GetInfoByDate(DateTime FD, DateTime TD)
        {
            return GetInfoList().Where(n => n.FDate >= FD && n.TDate <= TD).ToList();
        }

        public Info GetInfoById(int InfoId)
        {
            return GetInfoList().FirstOrDefault(n => n.InfoId == InfoId);
        }
        #endregion

        #region "Category"
        public List<Category> GetCategoryList()
        {
            return _ctx.Categories.ToList();
        }

        public int AddCategory(Category category)
        {
            _ctx.Categories.Add(category);
            _ctx.SaveChanges();
            return _ctx.Categories.Max(c => c.CategoryId);
        }

        public bool UpdateCategory(Category category)
        {
            _ctx.Entry(category).State = System.Data.Entity.EntityState.Modified;
            _ctx.SaveChanges();
            return true;
        }

        public bool DeleteCategory(int categoryId)
        {
            Category category = GetCategoryById(categoryId);
            if (category != null)
            {
                _ctx.Entry(category).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public Category GetCategoryById(int categoryId)
        {
            return GetCategoryList().FirstOrDefault(c => c.CategoryId == categoryId);
        }

        public List<CategoryVM> GetCategoryVMList()
        {
            List<CategoryVM> result = new List<CategoryVM>();
            List<Category> categories = GetCategoryList();
            foreach (Category c in categories)
            {
                result.Add(new CategoryVM()
                {
                    Category = new Category() { CategoryId = c.CategoryId, Description = c.Description },
                    IsSelected = false
                });
            }
            return result;
        }
        #endregion

        #region "Intimacy"
        public List<Intimacy> GetIntimacyList()
        {
            return _ctx.Intimacies.ToList();
        }

        public int AddIntimacy(Intimacy intimacy)
        {
            _ctx.Intimacies.Add(intimacy);
            _ctx.SaveChanges();
            return _ctx.Intimacies.Max(i => i.IntimacyId);
        }

        public bool UpdateIntimacy(Intimacy intimacy)
        {
            _ctx.Entry(intimacy).State = System.Data.Entity.EntityState.Modified;
            _ctx.SaveChanges();
            return true;
        }
        public bool DeleteIntimacy(int intimacyId)
        {
            Intimacy intimacy = GetIntimacyById(intimacyId);
            if (intimacy != null)
            {
                _ctx.Entry(intimacy).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public Intimacy GetIntimacyById(int intimacyId)
        {
            return GetIntimacyList().FirstOrDefault(i => i.IntimacyId == intimacyId);
        }

        public List<IntimacyVM> GetIntimacyVMList()
        {
            List<IntimacyVM> result = new List<IntimacyVM>();
            List<Intimacy> intimacies = GetIntimacyList();
            foreach (Intimacy i in intimacies)
            {
                result.Add(new IntimacyVM()
                {
                    Intimacy = new Intimacy() { IntimacyId = i.IntimacyId, Description = i.Description },
                    IsSelected = false
                });
            }
            return result;
        }
        #endregion

        #region "Story"
        public List<Story> GetStoryList()
        {
            return _ctx.Stories.ToList();
        }

        public int AddStory(StoryVM storyVM)
        {
            Story story = new Story
            {

                StoryId = storyVM.StoryId,
                CategoryId = storyVM.CategoryId,
                IntimacyId = storyVM.IntimacyId,
                Title = storyVM.Title,
                Content = storyVM.Content,
                HashtagWord = storyVM.HashtagWord,
                Picture = storyVM.Picture,
                NoView = storyVM.NoView,
                ApproveStatus = "P",
                UserId = storyVM.UserId,

                Category = GetCategoryById(storyVM.CategoryId),
                Intimacy = GetIntimacyById(storyVM.IntimacyId),

                Hashtags = storyVM.Hashtags
            };

            _ctx.Stories.Add(story);
            _ctx.SaveChanges();

            AddHashtagByStory(story);
            return _ctx.Stories.Max(s => s.StoryId);
        }

        public bool UpdateStory(StoryVM storyVM)
        {
            Story story = new Story
            {

                StoryId = storyVM.StoryId,
                CategoryId = storyVM.CategoryId,
                IntimacyId = storyVM.IntimacyId,
                Title = storyVM.Title,
                Content = storyVM.Content,
                HashtagWord = storyVM.HashtagWord,
                Picture = storyVM.Picture,
                NoView = storyVM.NoView,
                ApproveStatus = "P",
                UserId = storyVM.UserId,

                Category = GetCategoryById(storyVM.CategoryId),
                Intimacy = GetIntimacyById(storyVM.IntimacyId),

                Hashtags = storyVM.Hashtags
            };

            _ctx.Entry(story).State = System.Data.Entity.EntityState.Modified;
            story.Hashtags.Clear() ;
            _ctx.SaveChanges();

            return AddHashtagByStory(story);            
        }

        public bool ApproveStory(int storyId, string feedback)
        {
            Story story = GetStoryById(storyId);
            story.ApproveStatus = "Y";
            story.Feedback = feedback;
            _ctx.Entry(story).State = System.Data.Entity.EntityState.Modified;            
            _ctx.SaveChanges();
            return true;
        }

        public bool DenyStory(int storyId, string feedback)
        {
            Story story = GetStoryById(storyId);
            story.Feedback = feedback;
            story.ApproveStatus = "N";
            _ctx.Entry(story).State = System.Data.Entity.EntityState.Modified;
            _ctx.SaveChanges();
            return true;
        }

        public bool DeleteStory(int storyId)
        {
            Story story = GetStoryById(storyId);
            if (story != null)
            {
                _ctx.Entry(story).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public List<Story> GetStoryForHome(List<int> categorySelected, List<int> intimacySelected, string userName, string title, string hashTag)
        {
            string[] hash = hashTag.Split();
            
            List<StoryVM> tmp = (from s in GetStoryVMList() .Where(s => s.ApproveStatus == "Y")
                                  where (categorySelected.Count > 0 ? categorySelected.Contains(s.CategoryId) : true) &&
                                    (intimacySelected.Count > 0 ? intimacySelected.Contains(s.IntimacyId) : true) &&
                                    (!string.IsNullOrEmpty(title) ? s.Title.Contains(title) : true) &&
                                    (!string.IsNullOrEmpty(userName) ? s.User.Nickname.Contains(userName) : true) &&
                                    (!string.IsNullOrEmpty(hashTag) && hash.Length > 0 ? s.Hashtags.Any(h => hash.Contains(h.Description)) : true)
                                  select s).ToList();

            List<Story> result = new List<Story>();
            foreach (StoryVM s in tmp)
            {
                result.Add(ConvertVMToStory(s));
            }          
            return result;
        }

        public List<StoryVM> GetStoryByStatus(string status)
        {
            List<Story> list = GetStoryList();

            List<StoryVM> result = new List<StoryVM>();
            List<Story> story = GetStoryList().Where(s => s.ApproveStatus == status).ToList();
            foreach (Story s in story)
            {
                result.Add(ConvertStoryToVM(s));
            }
            return result;
        }

        public List<Story> GetStoryByUserId(string userId)
        {
            return GetStoryList().Where(s => s.UserId == userId).ToList();
        }

        public Story GetStoryById(int storyId)
        {
            return GetStoryList().FirstOrDefault(s => s.StoryId == storyId);
        }

        public List<StoryVM> GetStoryVMList()
        {
            List<Story> story = GetStoryList();
            List<StoryVM> storyVM = new List<StoryVM>();
            foreach(Story s in story)
            {
                storyVM.Add(ConvertStoryToVM(s));
            }
            return storyVM;
        }

        public StoryVM GetStoryVMById(int storyId)
        {
            Story story = GetStoryList().FirstOrDefault(s => s.StoryId == storyId);
            return ConvertStoryToVM(story);
        }

        public StoryVM ConvertStoryToVM(Story story)
        {
            StoryVM storyVM = new StoryVM
            {

                StoryId = story.StoryId,
                CategoryId = story.CategoryId,
                IntimacyId = story.IntimacyId,
                Title = story.Title,
                Content = story.Content,
                HashtagWord = story.HashtagWord,
                Picture = story.Picture,
                NoView = story.NoView,
                ApproveStatus = story.ApproveStatus,
                UserId = story.UserId,

                Category = GetCategoryById(story.CategoryId),
                Intimacy = GetIntimacyById(story.IntimacyId),
                User = GetUserById(story.UserId),
                Hashtags = story.Hashtags
            };
            
            return storyVM;
        }

        public Story ConvertVMToStory(StoryVM storyVM)
        {
            Story story = new Story
            {

                StoryId = storyVM.StoryId,
                CategoryId = storyVM.CategoryId,
                IntimacyId = storyVM.IntimacyId,
                Title = storyVM.Title,
                Content = storyVM.Content,
                HashtagWord = storyVM.HashtagWord,
                Picture = storyVM.Picture,
                NoView = storyVM.NoView,
                ApproveStatus = storyVM.ApproveStatus,
                UserId = storyVM.UserId,

                Category = GetCategoryById(storyVM.CategoryId),
                Intimacy = GetIntimacyById(storyVM.IntimacyId),
                
                Hashtags = storyVM.Hashtags
            };
            return story;
        }

        public List<StoryVM> GetStoryVMByUserId(string userId)
        {
            List<StoryVM> result = new List<StoryVM>();
            List<Story> story = GetStoryList().Where(s => s.UserId == userId).ToList();
            foreach (Story s in story)
            {
                result.Add(ConvertStoryToVM(s));
            }
            return result;
        }
        #endregion

        #region "Comment"
        public List<Comment> GetCommentList()
        {
            return _ctx.Comments.ToList();
        }

        public Comment GetCommentById(int commentId)
        {
            return GetCommentList().FirstOrDefault(c => c.CommentId == commentId);
        }

        public List<Comment> GetCommentByStory(int storyId)
        {
            return GetCommentList().Where(c => c.StoryId == storyId).ToList();
        }

        public int AddComment(Comment comment)
        {
            _ctx.Comments.Add(comment);
            _ctx.SaveChanges();
            return _ctx.Comments.Max(c => c.CommentId);
        }

        public bool UpdateComment(Comment comment)
        {
            _ctx.Entry(comment).State = System.Data.Entity.EntityState.Modified;
            _ctx.SaveChanges();
            return true;
        }
        public bool DeleteCommentById(int commentId)
        {
            Comment comment = GetCommentById(commentId);
            if (comment != null)
            {
                _ctx.Entry(comment).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public bool DeleteCommentByStory(int storyId)
        {
            List<Comment> comment = GetCommentByStory(storyId);
            if (comment.Any())
            {
                _ctx.Entry(comment).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }
        #endregion

        #region "Hashtag"
        public List<Hashtag> GetHashtagList()
        {
            return _ctx.Hashtags.ToList();
        }

        public int AddHashtag(Hashtag hashtag)
        {
            _ctx.Hashtags.Add(hashtag);
            _ctx.SaveChanges();
            return _ctx.Hashtags.Max(h => h.HashtagId);
        }

        public bool AddHashtagByStory(Story story)
        {
            string[] tmpHashtag = story.HashtagWord.Split();
            for (int i = 0; i < tmpHashtag.Length; i++)
            {
                if (tmpHashtag[i].Trim() == "") continue;

                Hashtag hashtag;
                if (!GetHashtagList().Any(h => h.Description == tmpHashtag[i]))
                {
                    hashtag = new Hashtag { Description = tmpHashtag[i] };
                    hashtag.Stories.Add(story);
                    _ctx.Hashtags.Add(hashtag);
                }
                else
                {
                    hashtag = GetHashtagList().FirstOrDefault(h => h.Description == tmpHashtag[i]);
                    hashtag.Stories.Add(story);
                    _ctx.Entry(hashtag).State = System.Data.Entity.EntityState.Modified;
                }
            }
            return _ctx.SaveChanges() > 0;
        }

        public bool UpdateHashtag(Hashtag hashtag)
        {
            _ctx.Entry(hashtag).State = System.Data.Entity.EntityState.Modified;
            _ctx.SaveChanges();
            return true;
        }

        public bool DeleteHashtagById(int hashtagId)
        {
            Hashtag hashtag = GetHashtagById(hashtagId);
            if (hashtag != null)
            {
                _ctx.Entry(hashtag).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public bool DeleteHashtagByStory(int storyId)
        {
            List<Hashtag> hashtag = GetHashtagByStory(storyId);
            if (hashtag.Any())
            {
                _ctx.Entry(hashtag).State = System.Data.Entity.EntityState.Deleted;
                _ctx.SaveChanges();
            }
            return true;
        }

        public Hashtag GetHashtagById(int hastagId)
        {
            return GetHashtagList().FirstOrDefault(h => h.HashtagId == hastagId);
        }

        public Hashtag GetHashtagByDesc(string description)
        {
            return GetHashtagList().FirstOrDefault(h => h.Description == description);
        }

        public List<Hashtag> GetHashtagByStory(int storyId)
        {
            return GetHashtagList().Where(h => h.Stories.Any(s => s.StoryId == storyId)).ToList();
        }
        #endregion

        #region "User"
        public List<User> GetUserList()
        {
            return _ctx.Users.ToList();
        }

        public UserVM AddUser(UserVM user, string role)
        {
            var userMgr = new UserManager<SCMS.Models.User>(new UserStore<SCMS.Models.User>(_ctx));

            //if user name or email not existed
            if (!userMgr.Users.Any(u => u.UserName == user.UserName))
            {
                user.IsActive = true;
                user.Nickname = role == Role.admin.ToString() ? user.UserName : user.Nickname;
                user.PasswordHash = userMgr.PasswordHasher.HashPassword(user.PasswordHash);
                userMgr.Create(ConvertVMToUser(user));

                var tmpuser = userMgr.Users.Single(u => u.UserName == user.UserName);
                if (!tmpuser.Roles.Any(r => r.RoleId == role))
                {
                    userMgr.AddToRole(tmpuser.Id, role);
                    return ConvertUserToVM(tmpuser);
                }
            }

            UserVM tmp = new UserVM();
            tmp.Result = ReturnSuccess();
            tmp.Result.Success = false;
            tmp.Result.ErrorMessage = "Cannot create user";
            return tmp;
        }
            
        public UserVM UpdateUser(UserVM user, string role)
        {
            var userMgr = new UserManager<User>(new UserStore<User>(_ctx));
            User userTmp = userMgr.FindById(user.Id);
            if (userTmp == null)
            {
                user.Result = ReturnSuccess();
                user.Result.Success = false;
                user.Result.ErrorMessage = "User not found";
                return user;
            }

            userTmp.Email = user.Email;
            userTmp.Phone = user.Phone;
            userTmp.Quote = user.Quote;
            userTmp.Nickname = role == Role.admin.ToString() ? user.UserName : user.Nickname;
            var result = userMgr.Update(userTmp);
            user.Result = ReturnSuccess();
            if (!result.Succeeded)
            {
                user.Result.Success = false;
                user.Result.ErrorMessage = "Cannot update profile";
            }
            
            return user;
        }

        public bool ChangePassword(string userName, string currentPassword, string newPassword)
        {
            var userMgr = new UserManager<User>(new UserStore<User>(_ctx));
            User user = userMgr.Find(userName, currentPassword);
            if (user != null)
            {
                user.PasswordHash = userMgr.PasswordHasher.HashPassword(newPassword);
                var result = userMgr.Update(user);
                return result.Succeeded;
            }
            return false;
        }

        public bool DeactivateUser(string userName)
        {
            var userMgr = new UserManager<User>(new UserStore<User>(_ctx));
            User user = userMgr.FindByName(userName);
            if (user == null)
            {
                return false;
            }
            user.IsActive = false;
            var result = userMgr.Update(user);
            return result.Succeeded;
        }


        public bool ReactivateUser(string userName)
        {
            var userMgr = new UserManager<User>(new UserStore<User>(_ctx));
            User user = userMgr.FindByName(userName);
            if (user == null)
            {
                return false;
            }
            user.IsActive = true;
            var result = userMgr.Update(user);
            return result.Succeeded;
        }

        public bool DeleteUser(string userId)
        {
            var userMgr = new UserManager<User>(new UserStore<User>(_ctx));
            User user = userMgr.FindById(userId);
            var logins = user.Logins;
            var rolesForUser = userMgr.GetRoles(userId);

            using (var transaction = _ctx.Database.BeginTransaction())
            {
                foreach (var login in logins.ToList())
                {
                    userMgr.RemoveLogin(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                }

                if (rolesForUser.Count() > 0)
                {
                    foreach (var item in rolesForUser.ToList())
                    {
                        // item should be the name of the role
                        var result = userMgr.RemoveFromRole(user.Id, item);
                    }
                }

                userMgr.Delete(user);
                transaction.Commit();
            }
            return true;
        }

        public List<User> GetUserListByRole(string role)
        {
            string roldId = _ctx.Roles.FirstOrDefault(r => r.Name == role).Id;
            return GetUserList().Where(u => u.Roles.Any(r => r.RoleId == roldId)).ToList();
            //return GetUserList().Where(u => u.Roles.Any( r => _ctx.Roles.Any(ro => ro.Name == role))).ToList();
            //s.Hashtags.Any(h => hash.Contains(h.Description)) : true)
        }

        public User GetUserById(string userId)
        {
            return GetUserList().FirstOrDefault(u => u.Id == userId);
        }

        public UserVM GetUserVMByUserName(string userName)
        {
            return ConvertUserToVM(GetUserByUserName(userName));
        }

        public User GetUserByUserName(string userName)
        {
            return GetUserList().FirstOrDefault(u => u.UserName == userName);
        }

        public User ConvertVMToUser(UserVM input)
        {
            User result = new User()
            {
                Id = input.Id,
                PasswordHash = input.PasswordHash,
                UserName = input.UserName,
                Nickname = input.Nickname,
                IsActive = input.IsActive,
                Email = input.Email,
                Phone = input.Phone,
                ProfilePic = input.ProfilePic,
                Quote = input.Quote
            };
            return result;
        }

        public UserVM ConvertUserToVM(User input)
        {
            UserVM result = new UserVM()
            {
                Id = input.Id,
                PasswordHash = input.PasswordHash,
                UserName = input.UserName,
                Nickname = input.Nickname,
                IsActive = input.IsActive,
                Email = input.Email,
                Phone = input.Phone,
                ProfilePic = input.ProfilePic,
                Quote = input.Quote,
                Result = ReturnSuccess()
            };

            return result;
        }

        #endregion        
    }
}