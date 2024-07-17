using Amazon.S3;
using Amazon.S3.Model;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        // old code
        public async Task SaveDataInDb(Dictionary<string, object> data)
        {
            try
            {
                if (data == null)
                {
                    //throw new ArgumentNullException(nameof(data));
                    var logContent = new Dictionary<string, object>
                     {
                    {
                            "message", "Database Messege No Available Any Data" },
                    };
                    await AppendLogToAll(logContent);

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
                        decimal paymentAmount = Convert.ToDecimal(paymentAmountObj);
                        if (paymentAmount != 0)
                        {
                            //
                            userData.BrokerId = "C010025282";
                        }
                        else
                        {
                            // Handle the case where paymentAmount is zero, if needed
                            userData.BrokerId = "SU10025292";
                        }
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

                    if (allObjectData.TryGetValue("payer_address_city", out object payer_address_city) && payerLastNameObj != null)
                    {
                        userData.PayerAddressCity = payer_address_city.ToString();
                    }

                    if (allObjectData.TryGetValue("payer_address_state", out object PayerAddressState) && payerLastNameObj != null)
                    {
                        userData.PayerAddressState = PayerAddressState.ToString();
                    }

                    if (allObjectData.TryGetValue("payer_phone", out object payerPhoneObj) && payerPhoneObj != null)
                    {
                        userData.payer_phone = payerPhoneObj.ToString();
                    }

                    if (allObjectData.TryGetValue("payer_address_zip", out object PayerAddressZip) && payerLastNameObj != null)
                    {
                        userData.PayerAddressZip = PayerAddressZip.ToString();
                    }

                    if (allObjectData.TryGetValue("payer_address_country", out object PayerAddressCountry) && payerLastNameObj != null)
                    {
                        userData.PayerAddressCountry = PayerAddressCountry.ToString();
                    }

                    if (allObjectData.TryGetValue("coupon", out object coupon_code) && payerLastNameObj != null)
                    {
                        userData.coupon_code = coupon_code.ToString();
                    }
                    List<Receiver> newrecevers = new List<Receiver>();
                    if (allObjectData.TryGetValue("receivers", out object receiversObj))
                    {
                        List<Receiver> allReceivers = new List<Receiver>();

                        // Check if receiversObj is a JSON string containing multiple arrays
                        if (receiversObj is string receiversJson)
                        {
                            // Split the JSON string by newlines to get each JSON array
                            string[] jsonArrays = receiversJson.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string jsonArray in jsonArrays)
                            {
                                List<Receiver> receivers = JsonConvert.DeserializeObject<List<Receiver>>(jsonArray);
                                allReceivers.AddRange(receivers);  // Add the deserialized list to the main list
                            }
                        }
                        // Check if receiversObj is a JArray containing multiple arrays
                        else if (receiversObj is JArray receiversArray)
                        {
                            foreach (var array in receiversArray)
                            {
                                List<Receiver> receivers = array.ToObject<List<Receiver>>();
                                allReceivers.AddRange(receivers);  // Add the deserialized list to the main list
                            }
                        }

                        // Add the combined list of Receiver objects to userData
                        if (allReceivers != null)
                        {
                            userData.Receiverss.AddRange(allReceivers);
                        }
                        else
                        {
                            // Handle the case where deserialization failed
                            Console.WriteLine("Deserialization failed.");
                        }


                    }
                }
                else
                {
                    //throw new ArgumentException("Missing or invalid 'All Object Data' in input dictionary.");
                    var logContent = new Dictionary<string, object>
                {
                    { "message", "Database Messege No Available Any Data" },

                };
                    await AppendLogToAll(logContent);
                }

                // Ensure you have the correct connection string name in your appsettings.json
                string connstring = _configuration.GetConnectionString("db");

                using (MySqlConnection conn = new MySqlConnection(connstring))
                {
                    await conn.OpenAsync();

                    string query = @"
                    INSERT INTO ETX_Prod (
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
                        payer_address_zip,
                        country,
                        payer_phone,
                        receiver,
                        role,
                        member_id,
                        paid,
                        transaction_id,
                        broker_id,
                        coupon_code

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
                        @payer_address_zip,
                        @country,
                        @payer_phone,
                        @receiver,
                        @role,
                        @member_id,
                        @paid,
                        @transaction_id,
                        @broker_id,
                        @coupon_code
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
                        command.Parameters.AddWithValue("@country", userData.PayerAddressCountry);
                        command.Parameters.AddWithValue("@payer_phone", userData.payer_phone);
                        command.Parameters.AddWithValue("@receiver", userData.Receiver);
                        command.Parameters.AddWithValue("@role", userData.Role);
                        command.Parameters.AddWithValue("@member_id", userData.MemberId);
                        command.Parameters.AddWithValue("@paid", userData.Paid);
                        command.Parameters.AddWithValue("@transaction_id", userData.TransactionId);
                        command.Parameters.AddWithValue("@broker_id", userData.BrokerId);
                        command.Parameters.AddWithValue("@coupon_code", userData.coupon_code);
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



        public async Task GetDataFromObject(Response response)
        {
            try
            {
                NewClass newClass = new NewClass();
                newClass = response.data;
                List<Receiver> receivers = new List<Receiver>();
                foreach (var member in response.data.receivers)
                {
                    receivers.Add(new Receiver
                    {
                        member_id = member.member_id,
                        role = member.role,
                        email = member.email,
                        amount = member.amount,
                        paid = member.paid
                    });
                }
                await SaveDataInDatabaseFromObject(newClass,receivers);
            }
            catch (Exception Ex)
            {

            }
        }

        public async Task SaveDataInDatabaseFromObject(NewClass newClass, List<Receiver> receivers)
        {
            string connstring = _configuration.GetConnectionString("db");

            using (MySqlConnection conn = new MySqlConnection(connstring))
            {
                bool isFirstRecord = true;
                try
                {
                    if (receivers.Count < 1)
                    {
                        string query = @"INSERT INTO ETX_Prod (product_id, product_name, test_mode, sale_id, payment_gateway, payment_amount, payment_time, payer_email, payer_first_name, payer_last_name, payer_address_street, payer_address_city, payer_address_state, payer_address_zip, country, payer_phone, broker_id, coupon_code,updated_in_blue) VALUES (@product_id, @product_name, @test_mode, @sale_id, @payment_gateway, @payment_amount, @payment_time, @payer_email, @payer_first_name, @payer_last_name, @payer_address_street, @payer_address_city, @payer_address_state, @payer_address_zip, @country, @payer_phone,  @broker_id, @coupon_code,@updated_in_blue)";
                        using (MySqlCommand command = new MySqlCommand(query, conn))
                        {
                            command.CommandTimeout = 3600;

                            command.Parameters.AddWithValue("@product_id", newClass.product_id);
                            command.Parameters.AddWithValue("@product_name", newClass.product_name);
                            command.Parameters.AddWithValue("@test_mode", newClass.test_mode);
                            command.Parameters.AddWithValue("@sale_id", newClass.sale_id);
                            command.Parameters.AddWithValue("@payment_gateway", newClass.payment_gateway);
                            command.Parameters.AddWithValue("@payment_amount", newClass.payment_amount);
                            command.Parameters.AddWithValue("@payment_time", newClass.payment_time);
                            command.Parameters.AddWithValue("@payer_email", newClass.payer_email);
                            command.Parameters.AddWithValue("@payer_first_name", newClass.payer_first_name);
                            command.Parameters.AddWithValue("@payer_last_name", newClass.payer_last_name);
                            command.Parameters.AddWithValue("@payer_address_street", newClass.payer_address_street);
                            command.Parameters.AddWithValue("@payer_address_city", newClass.payer_address_city);
                            command.Parameters.AddWithValue("@payer_address_state", newClass.payer_address_state);
                            command.Parameters.AddWithValue("@payer_address_zip", newClass.payer_address_zip);
                            command.Parameters.AddWithValue("@country", newClass.payer_address_country);
                            command.Parameters.AddWithValue("@payer_phone", newClass.payer_phone);


                            decimal paymentAmount = Convert.ToDecimal(newClass.payment_amount);
                            if (paymentAmount != 0)
                            {
                                command.Parameters.AddWithValue("@broker_id", "C010025282");
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@broker_id", "SU10025292");
                            }

                            command.Parameters.AddWithValue("@coupon_code", newClass.coupon);
                            command.Parameters.AddWithValue("@updated_in_blue", 0);
                            

                            if (conn.State != System.Data.ConnectionState.Open)
                            {
                                await conn.OpenAsync();
                            }

                            // Execute query
                            int i = await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    { 
                    foreach (var item in receivers)
                    {
                            string query = @"INSERT INTO ETX_Prod (product_id, product_name, test_mode, sale_id, payment_gateway, payment_amount, payment_time, payer_email, payer_first_name, payer_last_name, payer_address_street, payer_address_city, payer_address_state, payer_address_zip, country, payer_phone, receiver, role, member_id,amount, paid,  broker_id, coupon_code,updated_in_blue) VALUES (@product_id, @product_name, @test_mode, @sale_id, @payment_gateway, @payment_amount, @payment_time, @payer_email, @payer_first_name, @payer_last_name, @payer_address_street, @payer_address_city, @payer_address_state, @payer_address_zip, @country, @payer_phone, @receiver, @role, @member_id,@amount, @paid, @broker_id, @coupon_code,@updated_in_blue)";
                            using (MySqlCommand command = new MySqlCommand(query, conn))
                            {
                                // Set command timeout if needed
                                command.CommandTimeout = 3600;

                                // Add parameters
                                command.Parameters.AddWithValue("@product_id", newClass.product_id);
                                command.Parameters.AddWithValue("@product_name", newClass.product_name);
                                command.Parameters.AddWithValue("@test_mode", newClass.test_mode);
                                command.Parameters.AddWithValue("@sale_id", newClass.sale_id);
                                command.Parameters.AddWithValue("@payment_gateway", newClass.payment_gateway);
                                command.Parameters.AddWithValue("@payment_amount", newClass.payment_amount);
                                command.Parameters.AddWithValue("@payment_time", newClass.payment_time);
                                command.Parameters.AddWithValue("@payer_email", newClass.payer_email);
                                command.Parameters.AddWithValue("@payer_first_name", newClass.payer_first_name);
                                command.Parameters.AddWithValue("@payer_last_name", newClass.payer_last_name);
                                command.Parameters.AddWithValue("@payer_address_street", newClass.payer_address_street);
                                command.Parameters.AddWithValue("@payer_address_city", newClass.payer_address_city);
                                command.Parameters.AddWithValue("@payer_address_state", newClass.payer_address_state);
                                command.Parameters.AddWithValue("@payer_address_zip", newClass.payer_address_zip);
                                command.Parameters.AddWithValue("@country", newClass.payer_address_country);
                                command.Parameters.AddWithValue("@payer_phone", newClass.payer_phone);

                                command.Parameters.AddWithValue("@receiver", item.email);
                                command.Parameters.AddWithValue("@role", item.role);
                                command.Parameters.AddWithValue("@member_id", item.member_id);
                                command.Parameters.AddWithValue("@amount", item.amount);
                                command.Parameters.AddWithValue("@paid", item.paid);

                                //command.Parameters.AddWithValue("@transaction_id", item.);

                                decimal paymentAmount = Convert.ToDecimal(newClass.payment_amount);
                                if (paymentAmount != 0)
                                {
                                    //
                                    //newClass.BrokerId = "C010025282";
                                    command.Parameters.AddWithValue("@broker_id", "C010025282");
                                }
                                else
                                {
                                    // Handle the case where paymentAmount is zero, if needed
                                    //userData.BrokerId = "SU10025292";
                                    command.Parameters.AddWithValue("@broker_id", "SU10025292");
                                }

                                command.Parameters.AddWithValue("@coupon_code", newClass.coupon);

                                if (isFirstRecord)
                                {
                                    command.Parameters.AddWithValue("@updated_in_blue", 0);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@updated_in_blue", 1);
                                }

                                if (conn.State != System.Data.ConnectionState.Open)
                                {
                                    await conn.OpenAsync();
                                }

                                // Execute query
                                 int i = await command.ExecuteNonQueryAsync();                            
                            }
                               isFirstRecord = false;
                        }
                    }
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
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        await conn.CloseAsync();
                    }
                }


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
