using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.Remoting;
using System.Text;
using Amazon.S3;
using System.Text.Json;
using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json.Linq;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using MySql.Data.MySqlClient;
using PaymentResponseApi.models;
using PaymentResponseApi.db_models;

namespace PaymentResponseApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IAmazonSecretsManager _secretsManager;
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration)
        {
            _secretsManager = new AmazonSecretsManagerClient();
            _configuration = configuration;
            _s3Client = new AmazonS3Client();
        }
        [HttpGet("Paymentget")]
        public IActionResult Get()
        {
            return Ok(new { Message = "Hello from AWS Lambda! Get Method" });
        }

        [HttpPost("Paymentresponse")]
        public async Task<IActionResult> Post()
        {
            var requestData = new Dictionary<string, object>();
            Dictionary<string, object> deserializedObject = new Dictionary<string, object>();
           
            try
            {
                if (HttpContext.Request.ContentType?.ToLower().Contains("application/json") == true)
                {
                    // Read JSON body asynchronously
                    using (var reader = new StreamReader(HttpContext.Request.Body))
                    {
                        var requestBody = await reader.ReadToEndAsync();
                        var data = JsonConvert.DeserializeObject<UserData>(requestBody);

                        deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);

                        requestData.Add("User_data", deserializedObject);

                    }
                }
                else
                {                    
                    var errorResponse = new
                    {
                        errorMessage = "Received request with incorrect Content-Type: " + HttpContext.Request.ContentType
                    };

                    requestData.Add("error :", errorResponse);
                    //await AppendLogToS3(requestData);
                    return BadRequest(errorResponse);
                }
                var response = new
                {
                    message = "Request processed successfully",
                    data = requestData
                };

                AddPaymnetDataInDb db = new AddPaymnetDataInDb(_configuration);
                await db.SaveDataInDb(requestData);
                //await AppendLogToS3(requestData);
                await AppendLogToAll(requestData);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error processing request data");

                //await WriteLog(ex.Message);
                var logContent = new Dictionary<string, object>
                {
                    { "message", "Internal server error" },
                    { "error", ex.Message },  // Add the exception message
                    { "stackTrace", ex.StackTrace }  // Optionally, add the stack trace
                };
                await AppendLogToAll(logContent);
                return StatusCode(500, "Internal server error");
            }
        }


        private async Task AppendLogToAll(Dictionary<string, object> logContent)
        {
            string bucketName = "speakert-unity"; // Corrected S3 bucket name
            string folderName = "payment_log/"; // Replace with your folder name in S3
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff"); // Unique timestamp for the log file name
            string key = $"{folderName}log-{timestamp}.txt"; // New log file key

            // Serialize the entire logContent dictionary to JSON
            var logJson = System.Text.Json.JsonSerializer.Serialize(logContent);

            // Add a date marker before the log entry
            string dateMarker = $"---- Log on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC ----";
            string logEntry = $"{dateMarker}\n{logJson}\n";

            // Convert log entry to byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(logEntry);

            // Upload the new log entry to S3
            using (var stream = new MemoryStream(byteArray))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = stream
                };

                await _s3Client.PutObjectAsync(putRequest);
            }
        }


    }
}
