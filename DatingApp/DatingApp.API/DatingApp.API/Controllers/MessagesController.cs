﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/user/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);
            if (messageFromRepo == null)
                return NotFound();
            return Ok(messageFromRepo);

        }
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId,
            [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturn>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
            return Ok(messages);
        }
        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturn>>(messageFromRepo);
            return Ok(messageThread);
        }



        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);
            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                return BadRequest("Could not find user");
            var message = _mapper.Map<Message>(messageForCreationDto);
            _repo.Add(message);

            if (await _repo.SaveAll())
            {
                var massageToReturn = _mapper.Map<MessageToReturn>(message);

                return CreatedAtRoute("GetMessage", new { id = message.Id }, massageToReturn);
            }


            throw new Exception("Creating the massage failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessage(id);
            if (messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }
            if (messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }
            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                _repo.Delete(messageFromRepo);
            }
            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message.");
        }

    }
}