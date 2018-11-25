/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Financial;

namespace Azos.Web.Pay.Mock
{
  /// <summary>
  /// Represents mock payment service (can be used for testing).
  /// Mock provider is driven by card pool configured in 'accounts" section (see sample config in ExternalCfg.LACONF)
  /// </summary>
  public sealed class MockSystem : PaySystem
  {
    #region Consts

      public const string CONFIG_ACCOUNTS_SECTION = "accounts";
      public const string CONFIG_ACCOUNT_DATA_NODE = "account-data";

      //public const string CONFIG_EMAIL_ATTR = "email";

      //public const string CONFIG_CARD_NUMBER_ATTR = "number";
      //public const string CONFIG_CARD_EXPYEAR_ATTR = "exp-year";
      //public const string CONFIG_CARD_EXPMONTH_ATTR = "exp-month";
      //public const string CONFIG_CARD_CVC_ATTR = "cvc";

      //public const string CONFIG_ACCOUNT_NUMBER_ATTR = "account-number";
      //public const string CONFIG_ROUTING_NUMBER_ATTR = "routing-number";
      //public const string CONFIG_ACCOUNT_TYPE_ATTR = "account-type";

    #endregion

    #region Nested classes

      private class Accounts: IConfigurable
      {
        #region Consts

          public const string CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_SECTION = "credit-card-correct";
          public const string CONFIG_ACCOUNTS_CREDIT_CARD_DECLINED_SECTION = "credit-card-declined";

          public const string CONFIG_ACCOUNTS_CREDIT_CARD_LUHN_ERROR_SECTION = "credit-card-luhn-error";
          public const string CONFIG_ACCOUNTS_CREDIT_CARD_CVC_ERROR_SECTION = "credit-card-cvc-error";
          public const string CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_WITH_ADDR_SECTION = "credit-card-correct-with-addr";

          public const string CONFIG_ACCOUNTS_DEBIT_BANK_CORRECT_SECTION = "debit-bank-correct";
          public const string CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_SECTION = "debit-card-correct";
          public const string CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_WITH_ADDR_SECTION = "debit-card-correct-with-address";

        #endregion

        #region .pvt/fields

          private List<AccountData> m_CreditCardsCorrect;
          private List<AccountData> m_CreditCardDeclined;
          private List<AccountData> m_CreditCardLuhnError;
          private List<AccountData> m_CreditCardCvcError;
          private List<AccountData> m_CreditCardCorrectWithAddr;

          private List<AccountData> m_DebitBankCorrect;
          private List<AccountData> m_DebitCardCorrect;
          private List<AccountData> m_DebitCardCorrectWithAddr;

        #endregion

        #region Properties

          public List<AccountData> CreditCardsCorrect { get { return m_CreditCardsCorrect != null ? m_CreditCardsCorrect : m_CreditCardsCorrect = new List<AccountData>(); } }
          public List<AccountData> CreditCardDeclined { get { return m_CreditCardDeclined != null ? m_CreditCardDeclined : m_CreditCardDeclined = new List<AccountData>(); } }

          public List<AccountData> CreditCardLuhnError { get { return m_CreditCardLuhnError != null ? m_CreditCardLuhnError : m_CreditCardLuhnError = new List<AccountData>(); } }
          public List<AccountData> CreditCardCvcError { get { return m_CreditCardCvcError != null ? m_CreditCardCvcError : m_CreditCardCvcError = new List<AccountData>(); } }
          public List<AccountData> CreditCardCorrectWithAddr { get { return m_CreditCardCorrectWithAddr != null ? m_CreditCardCorrectWithAddr : m_CreditCardCorrectWithAddr = new List<AccountData>(); } }

          public List<AccountData> DebitBankCorrect { get { return m_DebitBankCorrect != null ? m_DebitBankCorrect : m_DebitBankCorrect = new List<AccountData>(); } }
          public List<AccountData> DebitCardCorrect { get { return m_DebitCardCorrect != null ? m_DebitCardCorrect : m_DebitCardCorrect = new List<AccountData>(); } }
          public List<AccountData> DebitCardCorrectWithAddr { get { return m_DebitCardCorrectWithAddr != null ? m_DebitCardCorrectWithAddr : m_DebitCardCorrectWithAddr = new List<AccountData>(); } }

        #endregion

        #region Config

          public void Configure(IConfigSectionNode node)
          {
            m_CreditCardsCorrect = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_SECTION]);
            m_CreditCardDeclined = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_DECLINED_SECTION]);

            m_CreditCardLuhnError = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_LUHN_ERROR_SECTION]);
            m_CreditCardCvcError = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_CVC_ERROR_SECTION]);
            m_CreditCardCorrectWithAddr = createAccounts(node[CONFIG_ACCOUNTS_CREDIT_CARD_CORRECT_WITH_ADDR_SECTION]);

            m_DebitBankCorrect = createAccounts(node[CONFIG_ACCOUNTS_DEBIT_BANK_CORRECT_SECTION]);
            m_DebitCardCorrect = createAccounts(node[CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_SECTION]);
            m_DebitCardCorrectWithAddr = createAccounts(node[CONFIG_ACCOUNTS_DEBIT_CARD_CORRECT_WITH_ADDR_SECTION]);
          }

          private List<AccountData> createAccounts(IConfigSectionNode node)
          {
            var accounts = new List<AccountData>();

            foreach (var nAcc in node.Children.Where(ch => ch.IsSameName(CONFIG_ACCOUNT_DATA_NODE)))
            {
              var acc = FactoryUtils.MakeAndConfigure<AccountData>(nAcc, typeof(AccountData));
              accounts.Add(acc);
            }

            return accounts;
          }

        #endregion
      }

    #endregion


    #region ctor

      public MockSystem(IApplication app) : base(app) { }

      public MockSystem(IApplicationComponent director) : base(director) { }

    #endregion

    #region .pvt/fields

      private IConfigSectionNode m_AccountsCfg;
      private Accounts m_Accounts;
      private MockWebTerminal m_WebTerminal;

    #endregion


    #region Properties

      public override string ComponentCommonName { get { return "mockpay"; }}

      [Config(CONFIG_ACCOUNTS_SECTION)]
      public IConfigSectionNode AccountsCfg
      {
        get { return m_AccountsCfg; }
        set
        {
          m_Accounts = FactoryUtils.MakeAndConfigure<Accounts>(value, typeof(Accounts));
          m_AccountsCfg = value;
        }
      }

      public override IPayWebTerminal WebTerminal
      {
        get
        {
          if (m_WebTerminal == null)
            m_WebTerminal = new MockWebTerminal(this);
          return m_WebTerminal;
        }
      }

    #endregion

    #region PaySystem implementation
      protected override ConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
      { return ConnectionParameters.Make<MockConnectionParameters>(paramsSection); }

      protected override PaySession DoStartSession(ConnectionParameters cParams, IPaySessionContext context = null)
      { return new MockSession(this, (MockConnectionParameters)cParams, context); }

      protected internal override Transaction DoTransfer(PaySession session, Account from, Account to, Amount amount, string description = null, object extraData = null)
      {
        var actualAccountData = session.FetchAccountData(to);

        if (actualAccountData == null)
        {
          StatTransferError();
          throw new PaymentMockException(StringConsts.PAYMENT_UNKNOWN_ACCOUNT_ERROR.Args(from) + this.GetType().Name + ".Transfer");
        }

        AccountData accountData = null;

        accountData = m_Accounts.DebitBankCorrect.FirstOrDefault(c => c.AccountNumber == actualAccountData.AccountID.ToString()
                                                && c.CardExpirationYear == actualAccountData.CardExpirationDate.Value.Year
                                                && c.CardExpirationMonth == actualAccountData.CardExpirationDate.Value.Month
                                                && c.CardVC == actualAccountData.CardVC);

        if (accountData != null)
        {
          var created = DateTime.Now;

          var taId = session.GenerateTransactionID(TransactionType.Transfer);

          var ta = new Transaction(taId, TransactionType.Transfer, TransactionStatus.Success, Account.EmptyInstance, to, this.Name, taId, created, amount, description: description, extraData: extraData);

          StatTransfer(amount);

          return ta;
        }

        accountData = m_Accounts.DebitCardCorrect.FirstOrDefault(c => c.AccountNumber == actualAccountData.AccountID.ToString()
                                                && c.CardExpirationYear == actualAccountData.CardExpirationDate.Value.Year
                                                && c.CardExpirationMonth == actualAccountData.CardExpirationDate.Value.Month
                                                && c.CardVC == actualAccountData.CardVC);

        if (accountData != null)
        {
          var created = DateTime.Now;

          var taId = session.GenerateTransactionID(TransactionType.Transfer);

          var ta = new Transaction(taId, TransactionType.Transfer, TransactionStatus.Success, Account.EmptyInstance, to, this.Name, taId, created, amount, description: description, extraData: extraData);

          StatTransfer(amount);

          return ta;
        }

        accountData = m_Accounts.DebitCardCorrectWithAddr.FirstOrDefault(c => c.AccountNumber == actualAccountData.AccountID.ToString()
                                                && c.CardExpirationYear == actualAccountData.CardExpirationDate.Value.Year
                                                && c.CardExpirationMonth == actualAccountData.CardExpirationDate.Value.Month
                                                && c.CardVC == actualAccountData.CardVC
                                                && c.BillingAddress1 != actualAccountData.BillingAddress.Address1
                                                && c.BillingAddress2 != actualAccountData.BillingAddress.Address2
                                                && c.BillingCountry != actualAccountData.BillingAddress.Country
                                                && c.BillingCity != actualAccountData.BillingAddress.City
                                                && c.BillingPostalCode != actualAccountData.BillingAddress.PostalCode
                                                && c.BillingRegion != actualAccountData.BillingAddress.Region
                                                && c.BillingEmail != actualAccountData.BillingAddress.EMail
                                                && c.BillingPhone != actualAccountData.BillingAddress.Phone);

        if (accountData != null)
        {
          var created = DateTime.Now;

          var taId = session.GenerateTransactionID(TransactionType.Transfer);

          var ta = new Transaction(taId, TransactionType.Transfer, TransactionStatus.Success, Account.EmptyInstance, to, this.Name, taId, created, amount, description: description, extraData: extraData);

          StatTransfer(amount);

          return ta;
        }

        StatTransferError();
        throw new PaymentException(StringConsts.PAYMENT_INVALID_CARD_NUMBER_ERROR + this.GetType().Name + ".Transfer");
      }

      protected internal override Transaction DoCharge(PaySession session, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
      {
        var fromActualData = session.FetchAccountData(from);

        if (fromActualData == null)
        {
          StatChargeError();
          throw new PaymentMockException(StringConsts.PAYMENT_UNKNOWN_ACCOUNT_ERROR.Args(from) + this.GetType().Name + ".Charge");
        }

        if (m_Accounts.CreditCardDeclined.Any(c => c.AccountNumber == fromActualData.AccountID.ToString()))
        {
          StatChargeError();
          throw new PaymentMockException(this.GetType().Name + ".Charge: card '{0}' declined".Args(fromActualData));
        }

        if (m_Accounts.CreditCardLuhnError.Any(c => c.AccountNumber == fromActualData.AccountID.ToString()))
        {
          StatChargeError();
          throw new PaymentMockException(this.GetType().Name + ".Charge: card number '{0}' is incorrect".Args(fromActualData));
        }


        AccountData foundAccount = null;

        foundAccount = m_Accounts.CreditCardsCorrect.FirstOrDefault(c => c.AccountNumber == fromActualData.AccountID.ToString());

        if (foundAccount != null)
        {
          if (foundAccount.CardExpirationYear != fromActualData.CardExpirationDate.Value.Year)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationDate.Value.Year, fromActualData.CardExpirationDate.Value.Month) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardExpirationMonth != fromActualData.CardExpirationDate.Value.Month)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationDate.Value.Year, fromActualData.CardExpirationDate.Value.Month) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardVC != fromActualData.CardVC)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_CVC_ERROR + this.GetType().Name + ".Charge");
          }

          var created = DateTime.UtcNow;

          var taId = session.GenerateTransactionID(TransactionType.Charge);

          var ta = new Transaction(taId, TransactionType.Charge, TransactionStatus.Success, from, to, this.Name, taId, created, amount, description: description, extraData: extraData);

          StatCharge(amount);

          return ta;
        }

        foundAccount = m_Accounts.CreditCardCorrectWithAddr.FirstOrDefault(c => c.AccountNumber == fromActualData.AccountID.ToString());

        if (foundAccount != null)
        {
          if (foundAccount.CardExpirationYear != fromActualData.CardExpirationDate.Value.Year)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationDate.Value.Year, fromActualData.CardExpirationDate.Value.Month) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardExpirationMonth != fromActualData.CardExpirationDate.Value.Month)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_EXPIRATION_DATE_ERROR
              .Args(fromActualData.CardExpirationDate.Value.Year, fromActualData.CardExpirationDate.Value.Month) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.CardVC != fromActualData.CardVC)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_CVC_ERROR.Args(fromActualData.CardVC) + this.GetType().Name + ".Charge");
          }

          if (foundAccount.BillingAddress1 != fromActualData.BillingAddress.Address1 ||
              foundAccount.BillingAddress2 != fromActualData.BillingAddress.Address2 ||
              foundAccount.BillingCountry != fromActualData.BillingAddress.Country ||
              foundAccount.BillingCity != fromActualData.BillingAddress.City ||
              foundAccount.BillingPostalCode != fromActualData.BillingAddress.PostalCode ||
              foundAccount.BillingRegion != fromActualData.BillingAddress.Region ||
              foundAccount.BillingEmail != fromActualData.BillingAddress.EMail ||
              foundAccount.BillingPhone != fromActualData.BillingAddress.Phone)
          {
            StatChargeError();
            throw new PaymentMockException(StringConsts.PAYMENT_INVALID_ADDR_ERROR + this.GetType().Name + ".Charge");
          }

          var created = DateTime.UtcNow;

          var taId = session.GenerateTransactionID(TransactionType.Charge);

          var ta = new Transaction(taId, TransactionType.Charge, TransactionStatus.Success, from, to, this.Name, taId, created, amount, description: description, extraData: extraData);

          StatCharge(amount);

          return ta;
        }

        throw new PaymentException(StringConsts.PAYMENT_INVALID_CARD_NUMBER_ERROR + this.GetType().Name + ".Charge");
      }

      protected internal override bool DoVoid(PaySession session, Transaction charge, string description = null, object extraData = null)
      {
        StatVoid(charge);
        return true;
      }

      protected internal override bool DoCapture(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
      {
        StatCapture(charge, amount);
        return true;
      }

      protected internal override bool DoRefund(PaySession session, Transaction charge, decimal? amount = null, string description = null, object extraData = null)
      {
        StatRefund(charge, amount);
        return true;
      }
    #endregion
  }
}
