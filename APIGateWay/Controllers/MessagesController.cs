using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace APIGateWay.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork uow, IMapper mapper)
        {
            this._uow = uow;
            this._mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessage)
        {
            var username = User.GetUsername();
            if (username == createMessage.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send message to yourself");
            }
            var sender = await _uow.UserRepository.GetUserByNameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByNameAsync(createMessage.RecipientUsername);

            if (recipient == null) return NotFound("Recipient not found");
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessage.Content
            };
            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }
            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _uow.MessageRepository.GetMessageForUser(messageParams);
            Response.AddPaginationHeader(new PaginationHeader( messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
            return Ok(messages);
        }

        //[HttpGet("thread/{username}")]
        //public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        //{
        //    var currentUsername = User.GetUsername(); 
        //    return Ok(await _uow.MessageRepository.GetMessageThread(currentUsername, username));
        //}


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _uow.MessageRepository.GetMessage(id);
           // if (message == null) return NotFound();
            if (message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Unauthorized();
            }
            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _uow.MessageRepository.DeleteMessage(message);
            }
            if (await  _uow.Complete()) return Ok();
            return BadRequest("Problem deleting the message");
        }
    }
}
