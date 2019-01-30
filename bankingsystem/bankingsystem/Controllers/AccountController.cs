using EBankingMain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.IO;
using System.Web.Script.Serialization;
using bankingsystem;
using System.Data.Objects;

namespace EBankingMain.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AccountController : ApiController
    {
        eBankingEntities entity = null;

        public AccountController()
        {
            entity = new eBankingEntities();
        }

        [HttpPost]
        public IHttpActionResult AddBeneficiary(BeneficiaryDto beneficiaryDto)
        {
            beneficiary addBeneficiary = new beneficiary
            {
                name = beneficiaryDto.name,
                nickname = beneficiaryDto.nickName,
                accountnumber = beneficiaryDto.accountNumber,
                ifsccode = beneficiaryDto.ifscCode,
                maxamount = beneficiaryDto.maxAmount,
                maxtransactions = beneficiaryDto.maxTransactions,
                address = beneficiaryDto.address,
                usersaccountnumber = beneficiaryDto.usersAccountNumber
            };

            entity.beneficiaries.Add(addBeneficiary);
            entity.SaveChanges();
            return Ok(addBeneficiary);
        }


        [HttpGet]
        public IHttpActionResult ViewBeneficiary(string accountNumber)
        {
            var beneficiaries = entity.beneficiaries.Where(x => x.usersaccountnumber == accountNumber).ToList();
            return Ok(beneficiaries);
        }


        [HttpGet]
        public IHttpActionResult CheckAccountExists(string accountNumber)
        {
            var isAccountExists = entity.users.Where(x => x.accountnumber == accountNumber).ToList();
            return Ok(isAccountExists.Count);
        }


        [HttpGet]
        public IHttpActionResult AddTransactionsForBankServices(string accountNumber, string message, string amount)
        {
            transaction addTransaction = new transaction();
            var checkSenderTransactions = entity.users.Where(x => x.accountnumber == accountNumber).OrderByDescending(x => x.id).First();
            var currentSenderBalance = checkSenderTransactions.accountbalance;

            try
            {
                addTransaction.name = message;
                addTransaction.senderaccountnumber = accountNumber;
                addTransaction.receiveraccountnumber = accountNumber;
                addTransaction.transactiondate = DateTime.Now;
                addTransaction.creditamount = "0";
                addTransaction.debitamount = amount;
                addTransaction.senderbalanceamount = (Convert.ToDouble(currentSenderBalance) - Convert.ToDouble(amount)).ToString();
                addTransaction.receiverbalanceamount = "0";
                entity.transactions.Add(addTransaction);
                entity.SaveChanges();


                var userAccount = entity.users.First(x => x.accountnumber == accountNumber);
                userAccount.accountbalance = addTransaction.senderbalanceamount;
                entity.SaveChanges();

                return Ok(addTransaction);
            }
            catch (Exception ex)
            {
                return Ok("Error");
            }
        }




        [HttpPost]
        public IHttpActionResult AddFirstTransactions(TransactionsDto transactionsDto)
        {
            transaction addTransaction = new transaction();
            try
            {
                addTransaction.name = transactionsDto.name;
                addTransaction.nickname = transactionsDto.nickName;
                addTransaction.senderaccountnumber = transactionsDto.senderaccountnumber;
                addTransaction.receiveraccountnumber = transactionsDto.receiveraccountnumber;
                addTransaction.ifsccode = transactionsDto.ifscCode;
                addTransaction.transactiondate = DateTime.Now;
                addTransaction.creditamount = transactionsDto.creditamount;
                addTransaction.debitamount = transactionsDto.debitamount;
                addTransaction.senderbalanceamount = transactionsDto.creditamount;
                addTransaction.receiverbalanceamount = "0"; 
                entity.transactions.Add(addTransaction);
                entity.SaveChanges();
                return Ok(addTransaction);
            }
            catch (Exception ex)
            {
                return Ok("Error");
            }
        }


        [HttpPost]
        public IHttpActionResult AddTransactionsForSameBank(TransactionsDto transactionsDto)
        {
            transaction addTransaction = new transaction();
            string currentSenderBalance = "";
            string currentReceiverBalance = "";
            try
            {
                var checkSenderTransactions = entity.users.Where(x => x.accountnumber == transactionsDto.senderaccountnumber).OrderByDescending(x=>x.id).First();
                currentSenderBalance = checkSenderTransactions.accountbalance;

                var checkReceiverTransactions = entity.users.Where(x => x.accountnumber == transactionsDto.receiveraccountnumber).OrderByDescending(x => x.id).First();
                currentReceiverBalance = checkReceiverTransactions.accountbalance;
            }
            catch (Exception ex)
            {
                currentSenderBalance = "0";
                currentReceiverBalance = "0";
            }
            finally
            {
                addTransaction.name = transactionsDto.name;
                addTransaction.nickname = transactionsDto.nickName;
                addTransaction.senderaccountnumber = transactionsDto.senderaccountnumber;
                addTransaction.receiveraccountnumber = transactionsDto.receiveraccountnumber;
                addTransaction.ifsccode = transactionsDto.ifscCode;
                addTransaction.transactiondate = DateTime.Now;
                addTransaction.creditamount = transactionsDto.creditamount;
                addTransaction.debitamount = transactionsDto.debitamount;
                addTransaction.senderbalanceamount = ((Convert.ToDouble(currentSenderBalance) + Convert.ToDouble(transactionsDto.creditamount)) - Convert.ToDouble(transactionsDto.debitamount)).ToString();
                addTransaction.receiverbalanceamount = ((Convert.ToDouble(currentReceiverBalance) + Convert.ToDouble(transactionsDto.debitamount))).ToString();
                entity.transactions.Add(addTransaction);
                entity.SaveChanges();

                var userAccount = entity.users.First(x => x.accountnumber == transactionsDto.senderaccountnumber);
                userAccount.accountbalance = addTransaction.senderbalanceamount;
                entity.SaveChanges();


                var userreceiverAccount = entity.users.First(x => x.accountnumber == transactionsDto.receiveraccountnumber);
                userreceiverAccount.accountbalance = addTransaction.receiverbalanceamount;
                entity.SaveChanges();

            }
            return Ok(addTransaction);
        }



        [HttpPost]
        public IHttpActionResult AddTransactionsForDifferentBank(TransactionsDto transactionsDto)
        {
            transaction addTransaction = new transaction();
            string currentSenderBalance = "";
            try
            {
                var checkSenderTransactions = entity.users.Where(x => x.accountnumber == transactionsDto.senderaccountnumber).OrderByDescending(x => x.id).First();
                currentSenderBalance = checkSenderTransactions.accountbalance;
            }
            catch (Exception ex)
            {
                currentSenderBalance = "0";
            }
            finally
            {
                addTransaction.name = transactionsDto.name;
                addTransaction.nickname = transactionsDto.nickName;
                addTransaction.senderaccountnumber = transactionsDto.senderaccountnumber;
                addTransaction.receiveraccountnumber = transactionsDto.receiveraccountnumber;
                addTransaction.ifsccode = transactionsDto.ifscCode;
                addTransaction.transactiondate = DateTime.Now;
                addTransaction.creditamount = transactionsDto.creditamount;
                addTransaction.debitamount = transactionsDto.debitamount;
                addTransaction.senderbalanceamount = ((Convert.ToDouble(currentSenderBalance) + Convert.ToDouble(transactionsDto.creditamount)) - Convert.ToDouble(transactionsDto.debitamount)).ToString();
                entity.transactions.Add(addTransaction);
                entity.SaveChanges();

                var userAccount = entity.users.First(x => x.accountnumber == transactionsDto.senderaccountnumber);
                userAccount.accountbalance = addTransaction.senderbalanceamount;
                entity.SaveChanges();
            }
            return Ok(addTransaction);
        }


        [HttpPost]
        public IHttpActionResult IssueCheckBook(CheckbookDto checkbookDto)
        {
            checkbook addCheckbook = new checkbook
            {
                usersaccountnumber = checkbookDto.usersAccountNumber,
                issuedate = DateTime.Now,
                numberofpages = checkbookDto.numberOfPages, 
                delivereddate = DateTime.Now.AddDays(7),
                isdelivered = false
            };

            entity.checkbooks.Add(addCheckbook);
            entity.SaveChanges();
            return Ok(addCheckbook);
        }

        [HttpPost]
        public IHttpActionResult UpdateCheckBookDeliveryStatus(CheckbookDto checkbookDto)
        {
            var checkBook = entity.checkbooks.First(x => x.usersaccountnumber == checkbookDto.usersAccountNumber);
            checkBook.isdelivered = checkbookDto.status;
            entity.SaveChanges();
            return Ok(checkBook);
        }


        [HttpGet]
        public IHttpActionResult ViewIssuedCheckBook(string accountNumber)
        {
            var checkBook = entity.checkbooks.Where(x => x.usersaccountnumber == accountNumber).ToList();
            return Ok(checkBook);
        }


        [HttpPost]
        public IHttpActionResult ApplyBankServices(ServicesDto servicesDto)
        {
            bankservice addBankService = null;
            try
            {
                addBankService = entity.bankservices.First(x => x.usersaccountnumber == servicesDto.usersAccountNumber);
                addBankService.services = servicesDto.services;
                entity.SaveChanges();
                return Ok(addBankService);

            }
            catch(Exception ex)
            {
                addBankService = new bankservice
                {
                    usersaccountnumber = servicesDto.usersAccountNumber,
                    services = servicesDto.services
                };
                entity.bankservices.Add(addBankService);
                entity.SaveChanges();
                return Ok(addBankService);
            }
        }

        [HttpGet]
        public IHttpActionResult ViewAppliedBankService(string accountNumber)
        {
            var bankservices = entity.bankservices.Where(x => x.usersaccountnumber == accountNumber).ToList();
            return Ok(bankservices);
        }



        [HttpGet]
        public IHttpActionResult GetTransactions(string accountNumber, string startDate, string endDate)
        {
            DateTime startDateTime = Convert.ToDateTime(startDate);
            DateTime endDateTime = Convert.ToDateTime(endDate);
            var record = (from log in entity.transactions
                          where (EntityFunctions.TruncateTime(log.transactiondate) >= EntityFunctions.TruncateTime(startDateTime.Date)
                          && EntityFunctions.TruncateTime(log.transactiondate) <= EntityFunctions.TruncateTime(endDateTime.Date))
                          && ((log.senderaccountnumber == accountNumber && log.senderaccountnumber != "" && log.receiveraccountnumber != "") || (log.receiveraccountnumber == accountNumber && log.receiveraccountnumber != "" && log.senderaccountnumber != ""))
                          select log);
            return Ok(record);
            //DateTime startDateTime = Convert.ToDateTime(startDate);
            //DateTime endDateTime = Convert.ToDateTime(endDate);
            //var record = (from log in entity.transactions
            //           where (log.transactiondate >= startDateTime.Date)
            //           && (log.transactiondate <= endDateTime.Date)
            //           && (log.senderaccountnumber == accountNumber && log.senderaccountnumber != "" && log.receiveraccountnumber != "") || (log.receiveraccountnumber == accountNumber && log.receiveraccountnumber != "" && log.senderaccountnumber != "")
            //              select log);
            //return Ok(record);
        }

        [HttpGet]
        public IHttpActionResult GetLastTenTransactions(string accountNumber)
        {
            var record = (from log in entity.transactions
                          where (log.senderaccountnumber == accountNumber && log.senderaccountnumber != "" && log.receiveraccountnumber != "") || (log.receiveraccountnumber == accountNumber && log.receiveraccountnumber != "" && log.senderaccountnumber != "")
                          select log);
            return Ok(record);
        }

        [HttpGet]
        public IHttpActionResult GetFinalLastTenTransactions(string accountNumber)
        {
            var record = (from log in entity.transactions
                          where (log.senderaccountnumber == accountNumber && log.senderaccountnumber != "" && log.receiveraccountnumber != "") || (log.receiveraccountnumber == accountNumber && log.receiveraccountnumber != "" && log.senderaccountnumber != "")
                          orderby log.id descending
                          select log).Take(10);

            return Ok(record);
        }

    }
}
