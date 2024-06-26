using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.User;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace DocProjDEVPLANT.Services.Mail;

public class EmailService : IEmailService
{
    private readonly IUserService _userService;
    private readonly string _from;
    private readonly string _smtpServer;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    
    public EmailService(IOptions<EmailConfig> configuration,IUserService userService)
    {
        _userService = userService;
        _from = configuration.Value.From;
        _smtpServer = configuration.Value.EmailHost;
        _port = configuration.Value.Port;
        _username = configuration.Value.EmailUserName;
        _password = configuration.Value.EmailPassword;
    }

    public async Task SendEmailAsync(UserModel user, TemplateModel template, byte[] pdfBytes)
    {
	    dynamic userData = JsonConvert.DeserializeObject<dynamic>(user.UserData);
	    string fullName = userData.client?.nume;
	    
            var pdfFileName = $"{template.Name}_{fullName}.pdf";

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_from));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Generated Document";

            var pdfAttachment = new MimePart()
            {
                Content = new MimeContent(new MemoryStream(pdfBytes), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = pdfFileName
            };
            
            var emailHtmlBody = @"
            <!DOCTYPE html>
            <html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"">
            <head>
                <title></title>
                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    * { box-sizing: border-box; }
                    body { margin: 0; padding: 0; }
                    a[x-apple-data-detectors], #MessageViewBody a { color: inherit !important; text-decoration: inherit !important; }
                    p { line-height: inherit; }
                    .desktop_hide, .desktop_hide table { mso-hide: all; display: none; max-height: 0px; overflow: hidden; }
                    .image_block img+div { display: none; }
                    @media (max-width:705px) {
                        .desktop_hide table.icons-inner { display: inline-block !important; }
                        .icons-inner { text-align: center; }
                        .icons-inner td { margin: 0 auto; }
                        .mobile_hide { display: none; }
                        .row-content { width: 100% !important; }
                        .stack .column { width: 100%; display: block; }
                        .mobile_hide { min-height: 0; max-height: 0; max-width: 0; overflow: hidden; font-size: 0px; }
                        .desktop_hide, .desktop_hide table { display: table !important; max-height: none !important; }
                        .row-3 .column-1 .block-3.spacer_block { height: 30px !important; }
                        .row-3 .column-1 .block-1.heading_block td.pad { padding: 10px 20px 20px !important; }
                        .row-3 .column-1 .block-2.paragraph_block td.pad { padding: 10px 20px !important; }
                    }
                </style>
            </head>
            <body style=""background-color: #eaeef0; margin: 0; padding: 20px; -webkit-text-size-adjust: none; text-size-adjust: none;"">
                <table class=""nl-container"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background-color: #eaeef0;"">
                    <tbody>
                        <tr>
                            <td>
                                <table class=""row row-3"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                    <tbody>
                                        <tr>
                                            <td>
                                                <table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background-color: #ffffff; color: #000000; width: 685px; margin: 0 auto;"" width=""685"">
                                                    <tbody>
                                                        <tr>
                                                            <td class=""column column-1"" width=""100%"" style=""font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top;"">
                                                                <table class=""heading_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                                                    <tr>
                                                                        <td class=""pad"" style=""padding-bottom:20px;padding-left:40px;padding-right:10px;padding-top:10px;text-align:center;width:100%;"">
                                                                            <h1 style=""margin: 0; color: #111418; direction: ltr; font-family: Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 38px; font-weight: 700; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 45.6px;"">
                                                                                <span class=""tinyMce-placeholder""></span>
                                                                            </h1>
                                                                        </td>
                                                                    </tr>
                                                                </table>
																<table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																	<tr>
																		<td style=""text-align: center; padding: 10px 0;"">
																			<a href=""/"" class=""logo"">
																				<img height=""80px"" src=""https://imgur.com/AyS5y7f.png"" />
																			</a>
																		</td>
																	</tr>
																</table>
                                                                <table class=""paragraph_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""word-break: break-word;"">
                                                                    <tr>
                                                                        <td class=""pad"" style=""padding-bottom:10px;padding-left:40px;padding-right:10px;padding-top:10px;"">
                                                                            <div style=""color:#6f7376;direction:ltr;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:150%;text-align:left;mso-line-height-alt:27px;"">
                                                                                <p style=""margin: 0; margin-bottom: 16px;"">Hello <b>{firstName}</b>,</p>
                                                                                <p style=""margin: 0; margin-bottom: 16px;"">Your <b>{documentName}</b> document for <b>{companyName}</b> has been successfully issued and is attached to this email.</p>
                                                                                <p style=""margin: 0; margin-bottom: 16px;"">If you have any questions or need further assistance, please do not hesitate to reach out.</p>
                                                                                <p style=""margin: 0; margin-bottom: 16px;"">Thank you for using our app!</p>
                                                                                <p style=""margin: 0; margin-bottom: 16px;"">Best regards,</p>
                                                                                <p style=""margin: 0;""><b>Team Contract Seal</b></p>
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                                <div class=""spacer_block block-3"" style=""height:40px;line-height:40px;font-size:1px;"">&#8202;</div>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>";
            
            // Replace placeholders with actual values
            emailHtmlBody = emailHtmlBody.Replace("{firstName}", user.UserName)
                .Replace("{documentName}", template.Name)
                .Replace("{companyName}", template.Company.Name);
            
            var htmlPart = new TextPart("html")
            {
                Text = emailHtmlBody
            };

            var multipart = new Multipart("mixed");
            multipart.Add(htmlPart);
            multipart.Add(pdfAttachment);

            email.Body = multipart;

            using (var smtp = new SmtpClient())
            {
                try
                {
                    await smtp.ConnectAsync(_smtpServer, _port, SecureSocketOptions.StartTls);
                    smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                    await smtp.AuthenticateAsync(_username, _password);
                    await smtp.SendAsync(email);
                }
                finally
                {
                    await smtp.DisconnectAsync(true);
                    smtp.Dispose();
                }
            }
            
    }
    
    public async Task SendInviteEmailAsync(string email, TemplateModel template, string inviteLink)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(MailboxAddress.Parse(_from));
        emailMessage.To.Add(MailboxAddress.Parse(email));
        emailMessage.Subject = "Document Completion Invitation";

        var emailHtmlBody = @"
        <!DOCTYPE html>
		<html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"">
		
		<head>
			<title></title>
			<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
			<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""><!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]--><!--[if !mso]><!--><!--<![endif]-->
			<style>
				* {{
					box-sizing: border-box;
				}}
		
				body {{
					margin: 0;
					padding: 0;
				}}
		
				a[x-apple-data-detectors] {{
					color: inherit !important;
					text-decoration: inherit !important;
				}}
		
				#MessageViewBody a {{
					color: inherit;
					text-decoration: none;
				}}
		
				p {{
					line-height: inherit
				}}
		
				.desktop_hide,
				.desktop_hide table {{
					mso-hide: all;
					display: none;
					max-height: 0px;
					overflow: hidden;
				}}
		
				.image_block img+div {{
					display: none;
				}}
		
				@media (max-width:705px) {{
					.desktop_hide table.icons-inner {{
						display: inline-block !important;
					}}
		
					.icons-inner {{
						text-align: center;
					}}
		
					.icons-inner td {{
						margin: 0 auto;
					}}
		
					.mobile_hide {{
						display: none;
					}}
		
					.row-content {{
						width: 100% !important;
					}}
		
					.stack .column {{
						width: 100%;
						display: block;
					}}
		
					.mobile_hide {{
						min-height: 0;
						max-height: 0;
						max-width: 0;
						overflow: hidden;
						font-size: 0px;
					}}
		
					.desktop_hide,
					.desktop_hide table {{
						display: table !important;
						max-height: none !important;
					}}
		
					.row-2 .column-1 .block-1.heading_block td.pad {{
						padding: 10px 20px 20px !important;
					}}
		
					.row-2 .column-1 .block-2.paragraph_block td.pad,
					.row-2 .column-1 .block-3.paragraph_block td.pad {{
						padding: 10px 20px !important;
					}}
		
					.row-2 .column-1 .block-5.spacer_block {{
						height: 30px !important;
					}}
				}}
			</style>
		</head>
		
		<body class=""body"" style=""background-color: #eaeef0; margin: 0; padding: 20px; -webkit-text-size-adjust: none; text-size-adjust: none;"">
							<table class=""row row-2"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
								<tbody>
									<tr>
										<td>
											<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; border-radius: 0; color: #000000; width: 685px; margin: 0 auto;"" width=""685"">
												<tbody>
													<tr>
														<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
															<table class=""heading_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																<tr>
																	<td class=""pad"" style=""padding-bottom:20px;padding-left:40px;padding-right:10px;padding-top:10px;text-align:center;width:100%;"">
																		<h1 style=""margin: 0; color: #111418; direction: ltr; font-family: Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 38px; font-weight: 700; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 45.6px;""><span class=""tinyMce-placeholder""></span></h1>
																	</td>
																</tr>
															</table>
															<table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																<tr>
																	<td style=""text-align: center; padding: 10px 0;"">
																		<a href=""/"" class=""logo"">
																			<img height=""80px"" src=""https://imgur.com/AyS5y7f.png"" />
																		</a>
																	</td>
																</tr>
															</table>
															<table class=""paragraph_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
																<tr>
																	<td class=""pad"" style=""padding-bottom:10px;padding-left:40px;padding-right:10px;padding-top:10px;"">
																		<div style=""color:#6f7376;direction:ltr;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:150%;text-align:left;mso-line-height-alt:27px;"">
																			<p style=""margin: 0; margin-bottom: 16px;"">Dear Sir/Madam,</p>
																			<p style=""margin: 0;"">We kindly request your prompt completion of the <b>{formName}</b> for <b>{companyName}</b>. Your timely response is greatly appreciated.</p>
																		</div>
																	</td>
																</tr>
															</table>
															<table class=""paragraph_block block-3"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
																<tr>
																	<td class=""pad"" style=""padding-bottom:10px;padding-left:40px;padding-right:10px;padding-top:10px;"">
																		<div style=""color:#6f7376;direction:ltr;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:150%;text-align:left;mso-line-height-alt:27px;"">
																			<p style=""margin: 0; margin-bottom: 16px;"">If you have any questions or require further assistance, please feel free to contact us.</p>
																			<p style=""margin: 0; margin-bottom: 16px;"">Thank you for using our application.</p>
																			<p style=""margin: 0; margin-bottom: 16px;"">Best regards,</p>
																			<p style=""margin: 0;""><b>Team Contract Seal</b></p>
																		</div>
																	</td>
																</tr>
															</table>
															<table class=""button_block block-4"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																<tr>
																	<td class=""pad"" style=""padding-bottom:25px;padding-left:20px;padding-right:20px;padding-top:30px;text-align:center;"">
																		<div class=""alignment"" align=""center""><!--[if mso]>
		<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" style=""height:50px;width:122px;v-text-anchor:middle;"" arcsize=""8%"" stroke=""false"" fillcolor=""#7747FF"">
		<w:anchorlock/>
		<v:textbox inset=""0px,0px,0px,0px"">
		<center dir=""false"" style=""color:#ffffff;font-family:Arial, sans-serif;font-size:20px"">
		<![endif]-->
																			<div style=""background-color:#071952;border-bottom:0px solid transparent;border-left:0px solid transparent;border-radius:4px;border-right:0px solid transparent;border-top:0px solid transparent;color:#ffffff;display:inline-block;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:20px;font-weight:400;mso-border-alt:none;padding-bottom:5px;padding-top:5px;text-align:center;text-decoration:none;width:auto;word-break:keep-all;"">
																				<span style=""padding-left:20px;padding-right:20px;font-size:20px;display:inline-block;letter-spacing:normal;"">
																				<span style=""word-break: break-word; line-height: 40px;"">
																				<a href=""{formLink}"" style=""color: #ffffff; text-decoration: none;"">Form link</a></span></span></div><!--[if mso]></center></v:textbox></v:roundrect><![endif]-->
																		</div>
																	</td>
																</tr>
															</table>
														</td>
													</tr>
												</tbody>
											</table>
										</td>
									</tr>
								</tbody>
							</table>
						</td>
					</tr>
				</tbody>
			</table><!-- End -->
		</body>
		
		</html>";

        emailHtmlBody = emailHtmlBody.Replace("{formName}", template.Name)
	        .Replace("{companyName}", template.Company.Name)
	        .Replace("{formLink}", inviteLink);

        var bodyBuilder = new BodyBuilder { HtmlBody = emailHtmlBody };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var smtp = new SmtpClient())
        {
            await smtp.ConnectAsync(_smtpServer, _port, SecureSocketOptions.StartTls);
            smtp.AuthenticationMechanisms.Remove("XOAUTH2");
            await smtp.AuthenticateAsync(_username, _password);
            await smtp.SendAsync(emailMessage);
            await smtp.DisconnectAsync(true);
        }
    }

    public async Task SendRegisterEmailAsync(string email,TemplateModel template, string registerLink)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(MailboxAddress.Parse(_from));
        emailMessage.To.Add(MailboxAddress.Parse(email));
        emailMessage.Subject = "Account Confirmation";
        
        if (template?.Company?.Name == null)
        {
	        throw new Exception("Template's company name is not available.");
        }

        var emailHtmlBody = @"
            <!DOCTYPE html>
			<html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"">
			
			<head>
				<title></title>
				<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
				<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""><!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]--><!--[if !mso]><!--><!--<![endif]-->
				<style>
					* {
						box-sizing: border-box;
					}
			
					body {
						margin: 0;
						padding: 0;
					}
			
					a[x-apple-data-detectors] {
						color: inherit !important;
						text-decoration: inherit !important;
					}
			
					#MessageViewBody a {
						color: inherit;
						text-decoration: none;
					}
			
					p {
						line-height: inherit
					}
			
					.desktop_hide,
					.desktop_hide table {
						mso-hide: all;
						display: none;
						max-height: 0px;
						overflow: hidden;
					}
			
					.image_block img+div {
						display: none;
					}
			
					@media (max-width:705px) {
						.desktop_hide table.icons-inner {
							display: inline-block !important;
						}
			
						.icons-inner {
							text-align: center;
						}
			
						.icons-inner td {
							margin: 0 auto;
						}
			
						.mobile_hide {
							display: none;
						}
			
						.row-content {
							width: 100% !important;
						}
			
						.stack .column {
							width: 100%;
							display: block;
						}
			
						.mobile_hide {
							min-height: 0;
							max-height: 0;
							max-width: 0;
							overflow: hidden;
							font-size: 0px;
						}
			
						.desktop_hide,
						.desktop_hide table {
							display: table !important;
							max-height: none !important;
						}
			
						.row-2 .column-1 .block-1.heading_block td.pad {
							padding: 10px 20px 20px !important;
						}
			
						.row-2 .column-1 .block-2.paragraph_block td.pad,
						.row-2 .column-1 .block-3.paragraph_block td.pad {
							padding: 10px 20px !important;
						}
			
						.row-2 .column-1 .block-5.spacer_block {
							height: 30px !important;
						}
					}
				</style>
			</head>
			
			<body class=""body"" style=""background-color: #eaeef0; margin: 0; padding: 20px; -webkit-text-size-adjust: none; text-size-adjust: none;"">
								<table class=""row row-2"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
									<tbody>
										<tr>
											<td>
												<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; border-radius: 0; color: #000000; width: 685px; margin: 0 auto;"" width=""685"">
													<tbody>
														<tr>
															<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
																<table class=""heading_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																	<tr>
																		<td class=""pad"" style=""padding-bottom:20px;padding-left:40px;padding-right:40px;padding-top:10px;text-align:center;width:100%;"">
																			<h1 style=""margin: 0; color: #111418; direction: ltr; font-family: Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 38px; font-weight: 700; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 45.6px;""><span class=""tinyMce-placeholder""></span></h1>
																		</td>
																	</tr>
																</table>
																<table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																	<tr>
																		<td style=""text-align: center; padding: 10px 0;"">
																			<a href=""/"" class=""logo"">
																				<img height=""80px"" src=""https://imgur.com/AyS5y7f.png"" />
																			</a>
																		</td>
																	</tr>
																</table>
																<table class=""paragraph_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
																	<tr>
																		<td class=""pad"" style=""padding-bottom:0px;padding-left:40px;padding-right:40px;padding-top:10px;"">
																			<div style=""color:#6f7376;direction:ltr;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:150%;text-align:left;mso-line-height-alt:27px;"">
																				<p style=""margin: 0; margin-bottom: 16px;"">Dear Sir/Madam,</p>
																				<p style=""margin: 0; margin-bottom: 16px;"">Thank you for completing the <b>{formName}</b> for <b>{companyName}</b>. We are excited to have you on board!</p>
																				<p style=""margin: 0;""></p>To enhance your experience and make future interactions with us even smoother, we invite you to create an account on our platform. By registering, you can save your information, track your activities, and enjoy a personalized experience tailored just for you.</p>
																			</div>
																		</td>
																	</tr>
																</table>
																<table class=""paragraph_block block-3"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
																	<tr>
																		<td class=""pad"" style=""padding-bottom:10px;padding-left:40px;padding-right:40px;padding-top:10px;"">
																			<div style=""color:#6f7376;direction:ltr;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:18px;font-weight:400;letter-spacing:0px;line-height:150%;text-align:left;mso-line-height-alt:27px;"">
																				<p style=""margin: 0; margin-bottom: 16px;"">If you have any questions or require further assistance, please feel free to contact us.</p>
																				<p style=""margin: 0; margin-bottom: 16px;"">Thank you for using our application.</p>
																				<p style=""margin: 0; margin-bottom: 16px;"">Best regards,</p>
																				<p style=""margin: 0;""><b>Team Contract Seal</b></p>
																			</div>
																		</td>
																	</tr>
																</table>
																<table class=""button_block block-4"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																	<tr>
																		<td class=""pad"" style=""padding-bottom:25px;padding-left:20px;padding-right:20px;padding-top:30px;text-align:center;"">
																			<div class=""alignment"" align=""center""><!--[if mso]>
			<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" style=""height:50px;width:122px;v-text-anchor:middle;"" arcsize=""8%"" stroke=""false"" fillcolor=""#7747FF"">
			<w:anchorlock/>
			<v:textbox inset=""0px,0px,0px,0px"">
			<center dir=""false"" style=""color:#ffffff;font-family:Arial, sans-serif;font-size:20px"">
			<![endif]-->
																				<div style=""background-color:#071952;border-bottom:0px solid transparent;border-left:0px solid transparent;border-radius:4px;border-right:0px solid transparent;border-top:0px solid transparent;color:#ffffff;display:inline-block;font-family:Helvetica Neue, Helvetica, Arial, sans-serif;font-size:20px;font-weight:400;mso-border-alt:none;padding-bottom:5px;padding-top:5px;text-align:center;text-decoration:none;width:auto;word-break:keep-all;""><span style=""padding-left:20px;padding-right:20px;font-size:20px;display:inline-block;letter-spacing:normal;""><span style=""word-break: break-word; line-height: 40px;""><a href=""{registerLink}"" style=""color: #ffffff; text-decoration: none;"">Register link</a></span></span></div><!--[if mso]></center></v:textbox></v:roundrect><![endif]-->
																			</div>
																		</td>
																	</tr>
																</table>
															</td>
														</tr>
													</tbody>
												</table>
											</td>
										</tr>
									</tbody>
								</table>
							</td>
						</tr>
					</tbody>
				</table><!-- End -->
			</body>
			
			</html>";
        
        emailHtmlBody = emailHtmlBody.Replace("{formName}", template.Name)
	        .Replace("{companyName}", template.Company.Name)
	        .Replace("{registerLink}", registerLink);

        var bodyBuilder = new BodyBuilder { HtmlBody = emailHtmlBody };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var smtp = new SmtpClient())
        {
            await smtp.ConnectAsync(_smtpServer, _port, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.AuthenticationMechanisms.Remove("XOAUTH2");
            await smtp.AuthenticateAsync(_username, _password);
            await smtp.SendAsync(emailMessage);
            await smtp.DisconnectAsync(true);
        }
    }
    
}