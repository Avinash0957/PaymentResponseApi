using Amazon.S3;
using Amazon.S3.Model;
using MySql.Data.MySqlClient;
using PaymentResponseApi.models;
using System.Text;

namespace PaymentResponseApi.db_models
{
    public class AddPaymnetDataInDb
    {

        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;
        public AddPaymnetDataInDb(IConfiguration configuration)
        {
            _configuration = configuration;
            _s3Client = new AmazonS3Client();
        }

        public async Task SaveDataInDb(Dictionary<string, object> data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                var userData = new UserData();

                // Map dictionary values to UserData properties
                if (data.TryGetValue("User_data", out object allObjectDataObj) && allObjectDataObj is Dictionary<string, object> allObjectData)
                {
                    if (allObjectData.TryGetValue("product_id", out object productIdObj) && productIdObj != null)
                    {
                        userData.ProductId = productIdObj.ToString();
                    }

                    if (allObjectData.TryGetValue("product_name", out object productNameObj) && productNameObj != null)
                    {
                        userData.ProductName = productNameObj.ToString();
                    }

                    if (allObjectData.TryGetValue("test_mode", out object testModeObj) && testModeObj != null)
                    {
                        userData.TestMode = Convert.ToChar(testModeObj);
                    }

                    if (allObjectData.TryGetValue("sale_id", out object saleIdObj) && saleIdObj != null)
                    {
                        userData.SaleId = saleIdObj.ToString();
                    }

                    if (allObjectData.TryGetValue("payment_gateway", out object paymentGatewayObj) && paymentGatewayObj != null)
                    {
                        userData.PaymentGateway = paymentGatewayObj.ToString();
                    }

                    if (allObjectData.TryGetValue("payment_amount", out object paymentAmountObj) && paymentAmountObj != null)
                    {
                        userData.PaymentAmount = Convert.ToDecimal(paymentAmountObj);
                    }

                    if (allObjectData.TryGetValue("payment_time", out object paymentTimeObj) && paymentTimeObj != null)
                    {
                        userData.PaymentTime = Convert.ToDateTime(paymentTimeObj);
                    }

                    if (allObjectData.TryGetValue("payer_email", out object payerEmailObj) && payerEmailObj != null)
                    {
                        userData.PayerEmail = payerEmailObj.ToString();
                    }

                    if (allObjectData.TryGetValue("payer_first_name", out object payerFirstNameObj) && payerFirstNameObj != null)
                    {
                        userData.PayerFirstName = payerFirstNameObj.ToString();
                    }

                    if (allObjectData.TryGetValue("payer_last_name", out object payerLastNameObj) && payerLastNameObj != null)
                    {
                        userData.PayerLastName = payerLastNameObj.ToString();
                    }
                }
                else
                {
                    throw new ArgumentException("Missing or invalid 'All Object Data' in input dictionary.");
                }

                // Ensure you have the correct connection string name in your appsettings.json
                string connstring = _configuration.GetConnectionString("db");

                using (MySqlConnection conn = new MySqlConnection(connstring))
                {
                    await conn.OpenAsync();

                    string query = @"
                    INSERT INTO PaymentDetails (
                        product_id, 
                        product_name, 
                        test_mode, 
                        sale_id, 
                        payment_gateway,
                        payment_amount, 
                        payment_time, 
                        payer_email, 
                        payer_first_name, 
                        payer_last_name,
                        payer_address_street,
                        payer_address_city,
                        payer_address_state,
                        payer_address_zip
                    ) VALUES (
                        @product_id, 
                        @product_name, 
                        @test_mode, 
                        @sale_id, 
                        @payment_gateway,
                        @payment_amount, 
                        @payment_time, 
                        @payer_email, 
                        @payer_first_name, 
                        @payer_last_name, 
                        @payer_address_street, 
                        @payer_address_city, 
                        @payer_address_state, 
                        @payer_address_zip
                    )";

                    using (MySqlCommand command = new MySqlCommand(query, conn))
                    {
                        // Set command timeout if needed
                        command.CommandTimeout = 3600;

                        // Add parameters
                        command.Parameters.AddWithValue("@product_id", userData.ProductId);
                        command.Parameters.AddWithValue("@product_name", userData.ProductName);
                        command.Parameters.AddWithValue("@test_mode", userData.TestMode);
                        command.Parameters.AddWithValue("@sale_id", userData.SaleId);
                        command.Parameters.AddWithValue("@payment_gateway", userData.PaymentGateway);
                        command.Parameters.AddWithValue("@payment_amount", userData.PaymentAmount);
                        command.Parameters.AddWithValue("@payment_time", userData.PaymentTime);
                        command.Parameters.AddWithValue("@payer_email", userData.PayerEmail);
                        command.Parameters.AddWithValue("@payer_first_name", userData.PayerFirstName);
                        command.Parameters.AddWithValue("@payer_last_name", userData.PayerLastName);
                        command.Parameters.AddWithValue("@payer_address_street", userData.PayerAddressStreet);
                        command.Parameters.AddWithValue("@payer_address_city", userData.PayerAddressCity);
                        command.Parameters.AddWithValue("@payer_address_state", userData.PayerAddressState);
                        command.Parameters.AddWithValue("@payer_address_zip", userData.PayerAddressZip);

                        try
                        {
                            // Ensure the connection is open
                            if (conn.State != System.Data.ConnectionState.Open)
                            {
                                await conn.OpenAsync();
                            }
                            // Execute query
                            await command.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            // Handle or log the exception as needed
                            Console.WriteLine($"An error occurred: {ex.Message}");
                            var logContent = new Dictionary<string, object>
                                {
                                    { "message", "Database Messege" },
                                    { "error", ex.Message },  // Add the exception message
                                    { "stackTrace", ex.StackTrace }  // Optionally, 
                                };
                                await AppendLogToAll(logContent);

                        }
                        finally
                        {
                            // Optionally close the connection if it should not remain open
                            if (conn.State == System.Data.ConnectionState.Open)
                            {
                                await conn.CloseAsync();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                var logContent = new Dictionary<string, object>
                {
                    { "message", "Database Messege" },
                    { "error", ex.Message },  // Add the exception message
                    { "stackTrace", ex.StackTrace }  // Optionally, add the stack trace
                };
                await AppendLogToAll(logContent);
            }

        }

        private async Task AppendLogToAll(Dictionary<string, object> logContent)
        {
            string bucketName = "speakert-unity"; // Corrected S3 bucket name
            string folderName = "payment_log/"; // Replace with your folder name in S3
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"); // Unique timestamp for the log file name
            string key = $"{folderName}log-{timestamp}-DB_error.txt"; // New log file key

            // Serialize the entire logContent dictionary to JSON
            var logJson = System.Text.Json.JsonSerializer.Serialize(logContent);

            // Convert log content to byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(logJson);

            // Upload log content to S3
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
