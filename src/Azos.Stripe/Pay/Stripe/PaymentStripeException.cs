/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.Net;

using Azos;
using Azos.Serialization.JSON;

namespace Azos.Web.Pay.Stripe
{
  /// <summary>
  /// Represents stripe specific payment exception
  /// </summary>
  [Serializable]
  public class PaymentStripeException: PaymentException
  {
    /// <summary>
    /// Constructs exception composing details from response, method specific error description and actual inner exception
    /// </summary>
    /// <param name="response">Response of failed request</param>
    /// <param name="stripeErrorMessage">Method specific error description</param>
    /// <param name="inner">Actual inner exception</param>
    /// <returns>Composed exception</returns>
    public static PaymentStripeException Compose(HttpWebResponse response, string stripeErrorMessage, Exception inner)
    {
      int statusCode = (int)response.StatusCode;

      string responseErrMsg = string.Empty;
      try
      {
        using(var reponseStream  = response.GetResponseStream())
        {
          using (var responseReader = new System.IO.StreamReader(reponseStream))
          {
            string responseStr = responseReader.ReadToEnd();
            dynamic responseObj = responseStr.JSONToDynamic();
            responseErrMsg = responseObj.error.message;
          }
        }
      }
      catch (Exception)
      {
        // dlatushkin 2014/04/07:
        // there is no way to test some cases (50X errors for example)
        // so try/catch is used to swallow exception
      }

      string specificError = System.Environment.NewLine;
      if (responseErrMsg.IsNotNullOrWhiteSpace())
        specificError += StringConsts.PAYMENT_STRIPE_ERR_MSG_ERROR.Args( responseErrMsg) + System.Environment.NewLine;

      specificError += stripeErrorMessage;

      PaymentStripeException ex = null;

      if (statusCode == 400)
        ex = new PaymentStripeException(StringConsts.PAYMENT_STRIPE_400_ERROR + specificError, inner);

      if (statusCode == 401)
        ex = new PaymentStripeException(StringConsts.PAYMENT_STRIPE_401_ERROR + specificError, inner);

      if (statusCode == 402)
        ex = new PaymentStripeException(StringConsts.PAYMENT_STRIPE_402_ERROR + specificError, inner);

      if (statusCode == 404)
        ex = new PaymentStripeException(StringConsts.PAYMENT_STRIPE_404_ERROR + specificError, inner);

      if (statusCode == 500 || statusCode == 502 || statusCode == 503 || statusCode == 504)
        ex = new PaymentStripeException(StringConsts.PAYMENT_STRIPE_50X_ERROR.Args(statusCode) + specificError, inner);

      return ex;
    }

    public PaymentStripeException(string message) : base(message) { }
    public PaymentStripeException(string message, Exception inner) : base(message, inner) { }
    protected PaymentStripeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
