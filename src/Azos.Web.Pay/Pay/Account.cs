/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Web.Pay
{

  /// <summary>
  /// Represents an account data vector that is -
  /// type of account identity (i.e. 'customer'), identity id (i.e. customer number '125') and
  /// account id within this identity (i.e. ref to customer #125 card '223322.....').
  /// May also represent temporary web terminal token generated by IWebPayTerminal in which case IsWebTerminalToken = true
  /// </summary>
  [Serializable]
  public struct Account : IEquatable<Account>
  {
    private static Account s_EmptyInstance = new Account(null, null, null);

    public static Account EmptyInstance { get { return s_EmptyInstance; } }

    public Account(string identity, object identityID, object accountID) : this()
    {
      Identity = identity;
      IdentityID = identityID;
      AccountID = accountID;
    }

    /// <summary>
    /// For example 'customer' - name of table.
    /// </summary>
    public string Identity { get; private set; }

    /// <summary>
    /// For example '125' - id of customer table row 125.
    /// </summary>
    public object IdentityID { get; private set; }

    /// <summary>
    /// Account id within identity id domain.
    /// For example '2' - id of method of payment for customer #125.
    /// </summary>
    public object AccountID { get; private set; }

    public bool IsEmpty { get { return Identity == null && IdentityID == null && AccountID == null; } }

    #region Object overrides

    public override string ToString()
    {
      if (IsEmpty)
        return "[EMPTY]";
      else
        return "Account({0}, {1}, {2})".Args(Identity, IdentityID, AccountID);
    }

    public override int GetHashCode()
    {
      return (Identity == null ? 0 : Identity.GetHashCode())
        ^ (IdentityID == null ? 0 : IdentityID.GetHashCode())
        ^ (AccountID == null ? 0 : AccountID.GetHashCode());
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Account)) return false;
      return Equals((Account)obj);
    }

    public bool Equals(Account other)
    {
      return object.Equals(Identity, other.Identity)
        && object.Equals(IdentityID, other.IdentityID)
        && object.Equals(AccountID, other.AccountID);
    }

    public static bool operator ==(Account account0, Account account1)
    {
      return account0.Equals(account1);
    }

    public static bool operator !=(Account account0, Account account1)
    {
      return !account0.Equals(account1);
    }

    #endregion

  }
}