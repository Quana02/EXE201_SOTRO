using System.Net;
using Microsoft.AspNetCore.Mvc;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Contact;
using SoTro_BE.Services;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private const string SystemContactEmail = "sotro.vn@gmail.com";
        private readonly IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("consultation")]
        public async Task<ActionResult<ApiResponse<bool>>> SendConsultationAsync([FromBody] ConsultationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.Fail("Vui lòng nhập đầy đủ họ tên, email, số điện thoại và nội dung tư vấn."));
            }

            var subject = $"Yêu cầu tư vấn Sổ Trọ - {request.FullName.Trim()}";
            var body = BuildConsultationEmailBody(request);

            await _emailService.SendEmailAsync(SystemContactEmail, subject, body);

            return Ok(ApiResponse<bool>.Ok("Đã gửi yêu cầu tư vấn. Sổ Trọ sẽ liên hệ lại với bạn sớm.", true));
        }

        private static string BuildConsultationEmailBody(ConsultationRequest request)
        {
            var fullName = WebUtility.HtmlEncode(request.FullName.Trim());
            var email = WebUtility.HtmlEncode(request.Email.Trim());
            var phone = WebUtility.HtmlEncode(request.Phone.Trim());
            var message = WebUtility.HtmlEncode(request.Message.Trim()).Replace("\n", "<br>");

            return $@"
                <div style=""font-family:Arial,sans-serif;color:#111827;line-height:1.6"">
                    <h2 style=""color:#004a98;margin-bottom:12px"">Yêu cầu tư vấn mới từ landing page</h2>
                    <table style=""border-collapse:collapse;width:100%;max-width:640px"">
                        <tr><td style=""padding:8px 0;font-weight:700"">Họ tên</td><td>{fullName}</td></tr>
                        <tr><td style=""padding:8px 0;font-weight:700"">Email</td><td>{email}</td></tr>
                        <tr><td style=""padding:8px 0;font-weight:700"">Số điện thoại</td><td>{phone}</td></tr>
                    </table>
                    <div style=""margin-top:18px"">
                        <div style=""font-weight:700;margin-bottom:6px"">Nội dung cần tư vấn</div>
                        <div style=""padding:14px;border:1px solid #d1d5db;border-radius:8px;background:#f9fafb"">{message}</div>
                    </div>
                </div>";
        }
    }
}
