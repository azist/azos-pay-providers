/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Conf;

namespace Azos.Web.Pay.Mock
{
  [Serializable]
  public class MockActualAccountData : IActualAccountData, IConfigurable
  {
    #region CONST

      private const string CONFIG_ACCOUNT_NODE = "account";

      private const string CONFIG_IDENTITY_ATTR = "identity";
      private const string CONFIG_IDENTITYID_ATTR = "identity-id";
      private const string CONFIG_ACCOUNTID_ATTR = "account-id";

    #endregion

    #region Static

      public static MockActualAccountData MakeAndConfigure(IConfigSectionNode node)
      {
        return FactoryUtils.MakeAndConfigure<MockActualAccountData>(node, typeof(MockActualAccountData));
      }

    #endregion

    #region ctor

      public MockActualAccountData() {}

    #endregion

    #region Properties

      [Config] public Account Account { get; set; }

      [Config]
      public AccountData AccountData
      {
        get
        {
          return m_AccountData;
        }
        set
        {
          m_AccountData = value;

          m_BillingAddress.Address1 = m_AccountData.BillingAddress1;
          m_BillingAddress.Address2 = m_AccountData.BillingAddress2;

          m_BillingAddress.City = m_AccountData.BillingCity;
          m_BillingAddress.Region = m_AccountData.BillingRegion;
          m_BillingAddress.PostalCode = m_AccountData.BillingPostalCode;
          m_BillingAddress.Country = m_AccountData.BillingCountry;
          m_BillingAddress.Phone = m_AccountData.BillingPhone;
          m_BillingAddress.EMail = m_AccountData.BillingEmail;

          m_ShippingAddress.Address1 = m_AccountData.ShippingAddress1;
          m_ShippingAddress.Address2 = m_AccountData.ShippingAddress2;

          m_ShippingAddress.City = m_AccountData.ShippingCity;
          m_ShippingAddress.Region = m_AccountData.ShippingRegion;
          m_ShippingAddress.PostalCode = m_AccountData.ShippingPostalCode;
          m_ShippingAddress.Country = m_AccountData.ShippingCountry;
          m_ShippingAddress.Phone = m_AccountData.ShippingPhone;
          m_ShippingAddress.EMail = m_AccountData.ShippingEmail;
        }
      }

      public string Identity { get; set; }

      public bool IsNew { get; set; }
      public object IdentityID { get; set; }

      public bool IsWebTerminal { get; set; }
      public object AccountID { get { return AccountData.AccountNumber; } set { AccountData.AccountNumber = value.ToString(); } }

      public string FirstName { get { return AccountData.FirstName; } set { AccountData.FirstName = value; } }
      public string MiddleName { get { return AccountData.MiddleName; } set { AccountData.MiddleName = value; } }
      public string LastName { get { return AccountData.LastName; } set { AccountData.LastName = value; } }

      public AccountType AccountType { get { return AccountData.AccountType; } set { AccountData.AccountType = value; } }

      public string AccountTitle { get { return AccountData.AccountTitle; } }

      public bool HadSuccessfullTransactions { get { return AccountData.HadSuccessfullTransactions; } set { AccountData.HadSuccessfullTransactions = value; } }
      public string IssuerID { get { return AccountData.IssuerID; } set { AccountData.IssuerID = value; } }
      public string IssuerName { get { return AccountData.IssuerName; } set { AccountData.IssuerName = value; } }
      public string IssuerPhone { get { return AccountData.IssuerPhone; } set { AccountData.IssuerPhone = value; } }
      public string IssuerEMail { get { return AccountData.IssuerEMail; } set { AccountData.IssuerEMail = value; } }
      public string IssuerUri { get { return AccountData.IssuerUri; } set { AccountData.IssuerUri = value; } }

      public string RoutingNumber { get { return AccountData.RoutingNumber; } set { AccountData.RoutingNumber = value; } }

      public string CardMaskedName { get { return AccountData.CardMaskedName; } set { AccountData.CardMaskedName = value; } }
      public string CardHolder { get { return AccountData.CardHolder; } set { AccountData.CardHolder = value; } }
      public DateTime? CardExpirationDate
      {
        get
        {
          if (!AccountData.CardExpirationYear.HasValue || !AccountData.CardExpirationMonth.HasValue)
            return null;
          return new DateTime(AccountData.CardExpirationYear.Value, AccountData.CardExpirationMonth.Value, 1);
        }
        set
        {
          if (value.HasValue) { AccountData.CardExpirationYear = value.Value.Year; AccountData.CardExpirationMonth = value.Value.Month; }
          else { AccountData.CardExpirationYear = null; AccountData.CardExpirationMonth = null; }
        }
      }
      public string CardVC { get { return AccountData.CardVC; } set { AccountData.CardVC = value; } }

      public bool IsCard { get { return AccountData.IsCard; } }

      public string Phone { get; set; }
      public string EMail { get; set; }

      public IAddress BillingAddress { get { return m_BillingAddress; } }

      public IAddress ShippingAddress { get { return m_ShippingAddress; } }

    #endregion

    #region Public methods

    public void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);

        var nAccountNode = node.Children.First(c => c.IsSameName(CONFIG_ACCOUNT_NODE));

        var identity = nAccountNode.AttrByName(CONFIG_IDENTITY_ATTR).Value;
        var identityID = nAccountNode.AttrByName(CONFIG_IDENTITYID_ATTR).Value;
        var accountID = nAccountNode.AttrByName(CONFIG_ACCOUNTID_ATTR).Value;

        Account = new Account(identity, identityID, accountID);
      }

    #endregion

    #region .pvt

    private AccountData m_AccountData;

      private readonly Address m_BillingAddress = new Address();
      private readonly Address m_ShippingAddress = new Address();

    #endregion
  }

  public class AccountData: IConfigurable
  {
    [Config] public string FirstName { get; set; }
    [Config] public string MiddleName { get; set; }
    [Config] public string LastName { get; set; }

    [Config] public AccountType AccountType { get; set; }

    public string AccountTitle { get { return string.Join(" ", new string[] { FirstName, MiddleName, LastName }.Where(s => s.IsNotNullOrWhiteSpace())); } }

    [Config] public bool HadSuccessfullTransactions { get; set; }
    [Config] public string IssuerID { get; set; }
    [Config] public string IssuerName { get; set; }
    [Config] public string IssuerPhone { get; set; }
    [Config] public string IssuerEMail { get; set; }
    [Config] public string IssuerUri { get; set; }
    [Config] public string AccountNumber { get; set; }
    [Config] public string RoutingNumber { get; set; }
    [Config] public string CardMaskedName { get; set; }
    [Config] public string CardHolder { get; set; }
    [Config] public int? CardExpirationYear { get; set; }
    [Config] public int? CardExpirationMonth { get; set; }
    [Config] public string CardVC { get; set; }

    public bool IsCard { get { return RoutingNumber.IsNullOrWhiteSpace(); } }

    [Config] public string BillingAddress1 { get; set; }
    [Config] public string BillingAddress2 { get; set; }
    [Config] public string BillingCity { get; set; }
    [Config] public string BillingRegion { get; set; }
    [Config] public string BillingPostalCode { get; set; }
    [Config] public string BillingCountry { get; set; }

    [Config] public string BillingPhone { get; set; }
    [Config] public string BillingEmail { get; set; }


    [Config] public string ShippingAddress1 { get; set; }
    [Config] public string ShippingAddress2 { get; set; }
    [Config] public string ShippingCity { get; set; }
    [Config] public string ShippingRegion { get; set; }
    [Config] public string ShippingPostalCode { get; set; }
    [Config] public string ShippingCountry { get; set; }

    [Config] public string ShippingPhone { get; set; }
    [Config] public string ShippingEmail { get; set; }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
