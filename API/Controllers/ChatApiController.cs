using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly IChatInterface _chat;

        public ChatApiController(IChatInterface chat)
        {
            _chat = chat;
        }


        #region SaveChat
        [HttpPost]
        public async Task<IActionResult> SaveChat([FromBody] Chat chat)
        {
            var chatId = await _chat.SaveChat(chat);
            if (chatId > 0)
                return Ok(new { chatId });
            
            return BadRequest(new { message = "Failed to save chat" });
        }
        #endregion


        #region GetChatHistory
        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory([FromQuery] string senderId, [FromQuery] string receiverId)
        {
            var chats = await _chat.GetChatHistory(senderId, receiverId);
            if (chats != null)
                return Ok(new { data = chats });
            
            return NotFound(new { message = "No chat history found" });
        }
        #endregion


        #region MarkChatAsRead
        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadChats(Guid userId)
        {
            var chats = await _chat.GetUnreadChats(userId);
            if (chats != null)
                return Ok(new { data = chats });
            
            return NotFound(new { message = "No unread messages" });
        }
        #endregion


        #region MarkChatAsRead
        [HttpPut("mark-read/{chatId}")]
        public async Task<IActionResult> MarkAsRead(int chatId)
        {
            var result = await _chat.MarkChatAsRead(chatId);
            if (result > 0)
                return Ok(new { message = "Message marked as read" });
            
            return NotFound(new { message = "Message not found" });
        }
        #endregion


        #region MarkAllAsRead
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid senderId, [FromQuery] Guid receiverId)
        {
            var result = await _chat.MarkAllChatsAsRead(senderId, receiverId);
            if (result)
                return Ok(new { message = "All messages marked as read" });
            
            return BadRequest(new { message = "Failed to mark messages as read" });
        }
        #endregion

    }
}