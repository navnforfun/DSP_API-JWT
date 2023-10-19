using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using DSP_API.Configurations.Filters;
using DSP_API.Models.Entity;
using DSP_API.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost]
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
            if(!IsBoxExits(boxId)){
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
        
        private bool IsBoxExits(int boxId)
        {
            var box = _context.Boxs.FirstOrDefault(b => b.Id == boxId);
            if (box == null)
            {
                return false;
            }
            return true;
        }

        private bool IsAuth(int possession)
        {
            if (possession == _UserId)
            {
                return true;
            }
            return false;
        }
    }
}