using cugonlineWebAPI.DTO;
using cugonlineWebAPI.Models;
using cugonlineWebAPI.VM;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;

namespace cugonlineWebAPI.Controllers
{
    [RoutePrefix("Api/businessusers")]
    public class BusinessUsersController : ApiController
    {
        testEntities geared4it_DB = new testEntities();

        public BusinessUsersController()
        {
            //constructor
        }
        #region C.R.U.D Business Users Information
        [Route("AddEditBusinessUsers")]
        [HttpPost]
        public async Task<BusinessUsersDTO> AddEditBusinessUsers(BusinessUsersDTO userInfo)
        {

            //validation
            if (userInfo == null) return new BusinessUsersDTO { CompanyCodePrimary = "Invalid format" };
            // ensure CellNumber Unique... Do If Exists Lookup
            if (userInfo.Id == 0)
            {//new user
                BusinessUser newUser = new BusinessUser
                {
                    FirstName = userInfo.FirstName,
                    Surname = userInfo.Surname,
                    UserName = userInfo.FirstName.Substring(0, 3) + "_" + userInfo.Surname.Substring(0, 3),
                    CellNumber = userInfo.CellNumber,//validate
                    UnlockCode = userInfo.UnlockCode,
                    BackupCellNumber = userInfo.BackupCellNumber,//validate
                    UUID = userInfo.UUID,//validate
                    CompanyCodePrimary = userInfo.CompanyCodePrimary,
                    Email = userInfo.Email,//validate
                    Password = userInfo.Password,
                    IsActive = userInfo.IsActive,
                    DateCreated = DateTime.Now,
                    LastSyncDate = DateTime.Now
                };

                geared4it_DB.BusinessUsers.Add(newUser);
            }
            else//Edit Business user
            {
                var updateUser = geared4it_DB.BusinessUsers.Find(userInfo.Id);

                //pass DTO model to BusinessUser Object
                //BusinessUser user = new BusinessUser
                // { 
                updateUser.FirstName = userInfo.FirstName;
                updateUser.Surname = userInfo.Surname;
                // UserName = userInfo.FirstName.Substring(0, 3) + "_" + userInfo.Surname.Substring(0, 3),
                updateUser.CellNumber = userInfo.CellNumber;//Admin Rights
                updateUser.UnlockCode = userInfo.UnlockCode;
                updateUser.BackupCellNumber = userInfo.BackupCellNumber;
                updateUser.UUID = userInfo.UUID;
                updateUser.CompanyCodePrimary = userInfo.CompanyCodePrimary;
                updateUser.Email = userInfo.Email;
                updateUser.Password = userInfo.Password;
                updateUser.IsActive = userInfo.IsActive;
                updateUser.DateCreated = DateTime.Now;
                updateUser.LastSyncDate = DateTime.Now;
                // };


                geared4it_DB.Entry(updateUser).State = System.Data.Entity.EntityState.Modified;
            }
            //save Add / Edit changes
            geared4it_DB.SaveChanges();

            //return model to Web / Mobile UI
            return new BusinessUsersDTO
            {
                Id = userInfo.Id,
                FirstName = userInfo.FirstName,
                Surname = userInfo.Surname,
                UserName = userInfo.FirstName.Substring(0, 3) + "_" + userInfo.Surname.Substring(0, 3),
                CellNumber = userInfo.CellNumber,
                UnlockCode = userInfo.UnlockCode,
                BackupCellNumber = userInfo.BackupCellNumber,
                UUID = userInfo.UUID,
                CompanyCodePrimary = userInfo.CompanyCodePrimary,
                Email = userInfo.Email,
                Password = userInfo.Password,
                IsActive = userInfo.IsActive,
                DateCreated = DateTime.Now,
                LastSyncDate = DateTime.Now
            };
        }

        /// <summary>
        /// Get Business user details
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        [Route("GetBusinessUserDetails")]
        [HttpGet]
        public async Task<BusinessUsersDTO> GetBusinessUserDetailsById(int userId)
        {
            try
            {
                var user = geared4it_DB.BusinessUsers.Find(userId);
                //pass DTO model to BusinessUser Object
                BusinessUsersDTO userDetails = new BusinessUsersDTO
                {
                    FirstName = user.FirstName,
                    Surname = user.Surname,
                    UserName = user.UserName,
                    CellNumber = user.CellNumber,//Admin Rights
                    UnlockCode = user.UnlockCode,
                    BackupCellNumber = user.BackupCellNumber,
                    UUID = user.UUID,
                    CompanyCodePrimary = user.CompanyCodePrimary,//MB001
                    Email = user.Email,
                    Password = user.Password,
                    IsActive = user.IsActive,
                    DateCreated = user.DateCreated,
                    LastSyncDate = user.LastSyncDate.Value
                    //user roles and rights
                };

                return userDetails;
            }
            catch (Exception ex)
            {
                //ToDo: log to traceability table repository
                return new BusinessUsersDTO{ ErrorMessage = "Error Getting User Details: " + ex.Message + " _StackTrace: " + ex.StackTrace };
            }
        }
        /// <summary>
        /// Deactivate User
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        [Route("DeactivateBusinessUser")]
        public async Task<BusinessUsersDTO> DeleteBusinessUserById(BusinessUsersDTO userInfo)
        {
            var updateUser = geared4it_DB.BusinessUsers.Find(userInfo.Id);
            //emailConfirmation(id);
            updateUser.IsActive = !updateUser.IsActive;

            geared4it_DB.Entry(updateUser).State = System.Data.Entity.EntityState.Modified;
            geared4it_DB.SaveChanges();

            return new BusinessUsersDTO { IsActive = false };
        }

        private void emailConfirmation(int id)
        {
            var u = geared4it_DB.BusinessUsers.Find(id);

            //send email to applicant
            var fromAddress = new MailAddress(u.Email, u.FirstName);
            string subject = " Online Registration request";
            string htmlBody;


            var smtp = new SmtpClient
            {
                Host = "relay-hosting.secureserver.net",
                Port = 25,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            //email applicant
            subject = " Online Registration completed.";
            htmlBody = "Good Day " + u.FirstName + " " + u.Surname +
                              " Your CUG online Registration is completed . login details: <br/> " +
                              " username requested : " + u.UserName + " <br/>" +
                              " password requested : " + u.Password + " <br/>" +
                              "Welcome to the CUG Family. <a href='https://cugonline.co.za/login'> </a>";



            var toAddress = new MailAddress(u.Email, u.FirstName);
            fromAddress = new MailAddress("cugonlinesa@gmail.com", "CUG Admin");
            using (var message = new MailMessage(toAddress, fromAddress)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
        #endregion


        #region C.R.U.D Business Clients / Customers
        //IsSupplier
        //IsNGO
        //IsXxx
        #endregion

        #region C.R.U.D Business Products
        [Route("AddEditProducts")]
        [HttpPost]
        public async Task<BusinessProductsDTO> AddEditProducts(BusinessProductsDTO productInfo)
        { 
            //validation
            if (productInfo == null) return new BusinessProductsDTO { ErrorMessage = "Invalid format" };

            try
            {
                if (productInfo.Id == 0)
                {//new user
                    BusinessProduct newProduct = new BusinessProduct
                    {
                        BusinessId = productInfo.BusinessId,
                        Reorder_level = productInfo.Reorder_level,
                        Name = productInfo.Name,//Get Name from SKUBarcode [Language localization] - Tooltip Alternate language List
                        SKUBarcode = productInfo.SKUBarcode,//Seperate Table
                        CostPrice = productInfo.CostPrice,
                        SellingPrice = productInfo.SellingPrice,//Admin Rights
                        CurrentSupplier = productInfo.CurrentSupplier,
                        Unit_id = productInfo.Unit_id,
                        Unit_in_stock = (float)productInfo.Unit_in_stock,
                        Category_id = productInfo.Category_id,//MB001
                        Unit_price = productInfo.Unit_price,
                        Discount_percentage = (float)productInfo.Discount_percentage,
                        //IsActive = product.IsActive,//default
                       DateCreated = productInfo.DateCreated,
                        LastSyncDate = productInfo.LastSyncDate,
                        //user roles and rights
                        User_id = productInfo.User_id
                    };

                    geared4it_DB.BusinessProducts.Add(newProduct);
                }
                else//Edit Product
                {
                    BusinessProductsDTO productUpdate = new BusinessProductsDTO
                    {
                        Name = productInfo.Name,//Get Name from SKUBarcode [Language localization] - Tooltip Alternate language List
                        SKUBarcode = productInfo.SKUBarcode,//Seperate Table
                        CostPrice = productInfo.CostPrice,
                        SellingPrice = productInfo.SellingPrice,//Admin Rights
                        CurrentSupplier = (long)productInfo.CurrentSupplier,
                        Unit_id = (int)productInfo.Unit_id,
                        Unit_in_stock = (float)productInfo.Unit_in_stock,
                        Category_id = productInfo.Category_id,//MB001
                        Unit_price = productInfo.Unit_price,
                        Discount_percentage = (float)productInfo.Discount_percentage,
                        IsActive = productInfo.IsActive,
                        DateCreated = productInfo.DateCreated,
                        LastSyncDate = DateTime.Now
                        //user roles and rights
                    };

                    geared4it_DB.Entry(productUpdate).State = System.Data.Entity.EntityState.Modified;
                }
                //save Add / Edit changes
                geared4it_DB.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }
          

            //return model to Web / Mobile UI
            return productInfo;
        }

        /// <summary>
        /// Gets Product details by Id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>Product Details DTO</returns>
        [Route("GetProductDetails")]
        [HttpGet]
        public async Task<BusinessProductsDTO> GetProductDetailsById(int productId)
        {
            try
            {
                var product = geared4it_DB.BusinessProducts.Find(productId);

                //pass Product Object to DTO model
                BusinessProductsDTO productInfo = new BusinessProductsDTO
                {
                    Name = product.Name,//Get Name from SKUBarcode [Language localization] - Tooltip Alternate language List
                    SKUBarcode = product.SKUBarcode,//Seperate Table
                    CostPrice = product.CostPrice,
                    SellingPrice = product.SellingPrice,//Admin Rights
                    CurrentSupplier = (long)product.CurrentSupplier,
                    Unit_id = (int)product.Unit_id,
                    Unit_in_stock = (float)product.Unit_in_stock,
                    Category_id = (int)(product.Category_id ?? null),//MB001
                    Unit_price = product.Unit_price,
                    Discount_percentage = (float)product.Discount_percentage,
                    IsActive = product.IsActive,
                    DateCreated = product.DateCreated,
                    LastSyncDate = product.LastSyncDate.Value
                    //user roles and rights
                };

                return productInfo;
            }
            catch (Exception ex)
            {
                HelpMeWith.Exceptions(ex);//ToDo: log Exception to traceability table repository                
                return new BusinessProductsDTO { ErrorMessage = "Error Getting User Details: " + ex.Message + " _StackTrace: " + ex.StackTrace };
            }
        }

        [Route("DeleteProductById")]
        public async Task<BusinessProductsDTO> DeleteProductById(BusinessProductsDTO productInfo)
        {
            var updateProduct = geared4it_DB.BusinessProducts.Find(productInfo.Id);

            //emailConfirmation(id);

            updateProduct.IsActive = !updateProduct.IsActive;

            geared4it_DB.Entry(updateProduct).State = System.Data.Entity.EntityState.Modified;
            geared4it_DB.SaveChanges();
            return new BusinessProductsDTO { IsActive = false };
        }

        #endregion

    }
}