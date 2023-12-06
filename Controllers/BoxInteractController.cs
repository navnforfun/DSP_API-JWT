using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using App;
using DSP_API.Configurations.Filters;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DSP_API.Controllers
{
    [ApiController]
    public class BoxInteractController : BaseController
    {
        private readonly DspApiContext _context;

        public BoxInteractController(DspApiContext context)
        {
            _context = context;
        }

        [HttpPost]
        [IsLogin()]
        public async Task<IActionResult> CreateComment(int boxId, string content)
        {

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Content is null");
            }
            User acc = await _context.Users.FirstOrDefaultAsync(a => a.Username == _Username);
            Comment comment = new Comment()
            {
                Content = content,
                DateCreated = DateTime.Now,
                BoxId = boxId,
                UserId = acc.Id
            };
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return Ok("Add comment successfully");
        }
        [IsLogin]
        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                return BadRequest("Can not find comment");
            }
            if (!IsAuth(comment.UserId))
            {
                return BadRequest("You are not the own");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok("Delete comment success");
        }
        [HttpGet]
        public async Task<IActionResult> GetListCommentBox(int boxId)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            if (box == null)
            {
                return BadRequest("Box is not exist");
            }
            var listComment = _context.Comments.Where(c => c.BoxId == boxId).Include(c => c.User).Select(c => new { c.Id, c.Content, c.DateCreated, userId = c.User.Id, c.User.Name, c.User.Username, c.User.Img }).ToList();

            return Ok(listComment);
        }
        [HttpGet]
        public async Task<IActionResult> GetLikeBox(int boxId)
        {
            if (!IsBoxExits(boxId))
            {
                return BadRequest("Box is not exitst");
            }
            var likeNum = _context.Votes.Where(v => v.BoxId == boxId).Count();
            return Ok(likeNum);
        }
        [HttpPost]
        [IsLogin]
        public async Task<IActionResult> LikeBox(int boxId)
        {
            if (!IsBoxExits(boxId))
            {
                return BadRequest("Box is not exits");
            }

            var vote = await _context.Votes.FirstOrDefaultAsync(v => v.BoxId == boxId && v.UserId == _UserId);
            if (vote == null)
            {
                _context.Votes.Add(new Vote
                {
                    BoxId = boxId,
                    UserId = _UserId

                });
                await _context.SaveChangesAsync();
                return Ok("Like success");
            }
            else
            {

                _context.Remove(vote);
                await _context.SaveChangesAsync();

                return Ok("Unlike success");
            }
        }
        [HttpGet]
        [SwaggerOperation(Summary = "Get list user have been share to the box ")]
        public async Task<IActionResult> GetUserShareInBox(int boxId)
        {

            var box = await _context.Boxs.Where(b => b.Id == boxId).FirstOrDefaultAsync();
            if (box == null)
            {
                return BadRequest("box is null");
            }
            if (box.UserId != _UserId)
            {
                return BadRequest("You have not permission");
            }
            var listUserShare = _context.BoxShares.Where(b => b.BoxId == boxId).Select(bs => new { bs.User.Id, bs.User.Img, bs.User.Name, bs.EditAccess });
            return Ok(listUserShare);
        }

        [HttpGet]
        [IsLogin()]
        [SwaggerOperation(Summary = "Get list box have been share to the User")]
        public async Task<IActionResult> GetBoxShareCurrentUser()
        {
            var listUserShare = _context.BoxShares.Where(b => b.UserId == _UserId);
            return Ok(listUserShare);

        }
        [HttpPost]
        [IsLogin()]
        public async Task<IActionResult> AddBoxShare(int boxId, string userName, bool EditAccess)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            
            if (box == null || user == null)
            {
                return BadRequest("Box or User is null|");
            }
            if (box.UserId == user.Id)
            {
                return Ok("You are the owner");
            }
            var listBoxShare = _context.BoxShares.Where(bs => bs.BoxId == boxId);
            if (!IsAuth(box.UserId))
            {
                return BadRequest("You have not permission");
            }
            if (listBoxShare.Any(l => l.UserId == user.Id))
            {
                return BadRequest("User is already exists ");
            };
            var boxShare = new BoxShare()
            {
                UserId = user.Id,
                BoxId = boxId,
                EditAccess = EditAccess
            };
            _context.Add(boxShare);
            await _context.SaveChangesAsync();
            return Ok("Successfully");
        }
        [HttpDelete]
        [IsLogin()]
        public async Task<IActionResult> RemoveBoxShare(int boxId, string userName)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);

            if (box == null || user == null)
            {
                return BadRequest("Box or User is null|");
            }
            var listBoxShare = _context.BoxShares.Where(bs => bs.BoxId == boxId);
            if (!IsAuth(box.UserId))
            {
                return BadRequest("You have not permission");
            }
            if (!listBoxShare.Any(l => l.UserId == user.Id))
            {
                return BadRequest("User have not been exists ");
            };
            var boxShare = await _context.BoxShares.FirstOrDefaultAsync(b => b.UserId == user.Id && b.BoxId == boxId);
            _context.Remove(boxShare);
            await _context.SaveChangesAsync();
            return Ok("Successfully");
        }
        [HttpGet]
        [IsLogin]
        public async Task<IActionResult> GetLinkShareBox(int boxId, bool shareEdit)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            if (box == null)
            {
                return BadRequest("Not found box");
            }
            if (box.UserId != _UserId)
            {
                return BadRequest("You have not permission");
            }
            var pass = _let.Random(30);
            if (shareEdit == true)
            {
                box.ShareEdit = pass;
            }
            else
            {
                box.ShareView = pass;
            }
            _context.Update(box);
            await _context.SaveChangesAsync();
            return Ok(pass);
        }

        [HttpGet]
        [IsLogin]
        public async Task<IActionResult> UpdateShareBoxByPass(int boxId, string pass, bool shareEdit)
        {
            var box = await _context.Boxs.FirstOrDefaultAsync(b => b.Id == boxId);
            if (box.UserId == _UserId)
            {
                return Ok("You are the owner");
            }
            if (box == null)
            {
                return BadRequest("Not found box");
            }
            var boxShare = await _context.BoxShares.FirstOrDefaultAsync(bs => bs.BoxId == boxId && bs.UserId == _UserId);
            if (boxShare != null)
            {
                if (boxShare.EditAccess == true)
                {
                    return Ok("You permission edit is already exist");
                }
                else
                {
                    if (shareEdit)
                    {
                        if (box.ShareEdit != pass)
                        {
                            return BadRequest("Pass wrong");
                        }

                        boxShare.EditAccess = true;
                        _context.Update(boxShare);
                        await _context.SaveChangesAsync();
                        return Ok("Update success");
                    }
                    else
                    {
                        return Ok("None update");
                    }
                }
            }
            else
            {
                if (!shareEdit)
                {
                    if (box.ShareView != pass)
                    {
                        return BadRequest("Pass wrong");
                    }
                    var newBoxShare = new BoxShare()
                    {
                        UserId = _UserId,
                        BoxId = boxId,
                        EditAccess = false
                    };
                    _context.Add(newBoxShare);
                }
                else
                {
                    if (box.ShareEdit != pass)
                    {
                        return BadRequest("Pass wrong");
                    }
                    var newBoxShare = new BoxShare()
                    {
                        UserId = _UserId,
                        BoxId = boxId,
                        EditAccess = true
                    };
                    _context.Add(newBoxShare);
                }




            }

            await _context.SaveChangesAsync();

            return Ok("Success");
        }
        [HttpGet]
        public async Task<IActionResult> FindBoxByTitle(string title)
        {



            var listBox = _context.Boxs.Where(b => b.Title.ToLower().Contains(title.ToLower())).Select(b => new { b.Id, b.Title, b.Img, b.Content });
            return Ok(listBox);
        }


        private bool IsBoxExits(int boxId)
        {
            var box = _context.Boxs.FirstOrDefault(b => b.Id == boxId);
            if (box == null)
            {
                return false;
            }
            return true;
        }

        private bool IsAuth(int? possession)
        {
            if (possession == _UserId)
            {
                return true;
            }
            return false;
        }
        private  bool IsInShareEdit(Box box)
        {
            var listUserShare = _context.BoxShares.Where(b => b.BoxId == box.Id).Select(b => b.UserId);
            if (listUserShare.Contains(_UserId))
            {
                var userShare = _context.BoxShares.FirstOrDefault(b => b.BoxId == box.Id && b.UserId == _UserId);
                if(userShare == null){
                    return false;
                }
                if(userShare.EditAccess == true){
                    return true;
                }
            }
            return false;
        }
        private bool IsInShare(Box box){
            var listUserShare = _context.BoxShares.Where(b => b.BoxId == box.Id).Select(b => b.UserId);
            if(listUserShare.Contains(_UserId)){
                return true;
            }
            return false;
        }
    }
}