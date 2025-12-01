using System.Security.Claims;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using QuestionService.Services;
using Wolverine;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(QuestionDbContext db, IMessageBus bus, TagService tagService) : ControllerBase
{
    public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto dto)
    {
        if (!await tagService.AreTagsValidAsync(dto.Tags))
            return BadRequest("Invalid tags");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue("name");

        if (userId is null || name is null) return BadRequest("cannot get user details");

        var question = new Question
        {
            Title = dto.Title,
            Content = dto.Content,
            TagSlugs = dto.Tags,
            AskerId = userId,
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync();

        await bus.PublishAsync(new QuestionCreated(question.Id, question.Title, question.Content, question.CreatedAt,
            question.TagSlugs));

        return Created($"/questions/{question.Id}", question);
    }

    // [HttpGet]
    // public async Task<ActionResult<PaginationResult<Question>>> GetQuestions([FromQuery]QuestionsQuery q)
    // {
    //     var query = db.Questions.AsQueryable(); 
    //
    //     if (!string.IsNullOrEmpty(q.Tag))
    //     {
    //         query = query.Where(x => x.TagSlugs.Contains(q.Tag));
    //     }
    //
    //     query = q.Sort switch
    //     {
    //         "newest" => query.OrderByDescending(x => x.CreatedAt),
    //         "active" => query.OrderByDescending(x => new[]
    //         {
    //             x.CreatedAt,
    //             x.UpdatedAt ?? DateTime.MinValue,
    //             x.Answers.Max(a => (DateTime?)a.CreatedAt) ?? DateTime.MinValue,
    //             x.Answers.Max(a => a.UpdatedAt) ?? DateTime.MinValue,
    //         }.Max()),
    //         "unanswered" => query.Where(x => x.AnswerCount == 0)
    //             .OrderByDescending(x => x.CreatedAt),
    //         _ => query.OrderByDescending(x => x.CreatedAt)
    //     };
    //
    //     var result = await query.ToPaginatedListAsync(q);
    //
    //     return result;
    // }

    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestion(string id)
    {
        var question = await db.Questions
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (question is null) return NotFound();

        await db.Questions.Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount,
                x => x.ViewCount + 1));

        return question;
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateQuestion(string id, CreateQuestionDto dto)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != question.AskerId) return Forbid();

        if (!await tagService.AreTagsValidAsync(dto.Tags))
            return BadRequest("Invalid tags");

        // var original = question.TagSlugs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        // var incoming = dto.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        //
        // var removed = original.Except(incoming, StringComparer.OrdinalIgnoreCase).ToArray();
        // var added = incoming.Except(original, StringComparer.OrdinalIgnoreCase).ToArray();

        // var sanitizer = new HtmlSanitizer();

        question.Title = dto.Title;
        // question.Content = sanitizer.Sanitize(dto.Content);
        question.Content = dto.Content;
        question.TagSlugs = dto.Tags;
        question.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // if (removed.Length > 0)
        // {
        //     await db.Tags
        //         .Where(t => removed.Contains(t.Slug) && t.UsageCount > 0)
        //         .ExecuteUpdateAsync(x => x.SetProperty(t => t.UsageCount, 
        //             t => t.UsageCount - 1));
        // }
        //
        // if (added.Length > 0)
        // {
        //     await db.Tags
        //         .Where(t => added.Contains(t.Slug))
        //         .ExecuteUpdateAsync(x => x.SetProperty(t => t.UsageCount, 
        //             t => t.UsageCount + 1));
        // }

        // await bus.PublishAsync(new QuestionUpdated(question.Id, question.Title, question.Content, 
        //     question.TagSlugs.AsArray()));

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteQuestion(string id)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != question.AskerId) return Forbid();

        db.Questions.Remove(question);
        await db.SaveChangesAsync();

        // await bus.PublishAsync(new QuestionDeleted(question.Id));

        return NoContent();
    }
}