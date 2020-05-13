﻿using System;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;

using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
// Required for the GetS3BucketsAsync example
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.IO;

public class CognitoHelper
{
    //private string POOL_ID = ConfigurationSettings.AppSettings["POOL_id"];
    //private string CLIENTAPP_ID = ConfigurationSettings.AppSettings["CLIENT_id"];
    //private string FED_POOL_ID = ConfigurationSettings.AppSettings["FED_POOL_id"];

    private string POOL_ID = ConfigurationSettings.AppSettings["APIPOOL_id"];
    private string CLIENTAPP_ID = ConfigurationSettings.AppSettings["APICLIENT_id"];
    private string FED_POOL_ID = ConfigurationSettings.AppSettings["APIFED_POOL_id"];

    private string CUSTOM_DOMAIN = ConfigurationSettings.AppSettings["CUSTOMDOMAIN"];

    private string REGION = ConfigurationSettings.AppSettings["AWSREGION"];

    public CognitoHelper()
    {
        
    }

    internal string GetCustomHostedURL()
    {
        return string.Format("https://{0}.auth.{1}.amazoncognito.com/login?response_type=code&client_id={2}&redirect_uri=https://sid343.reinvent-workshop.com/", CUSTOM_DOMAIN, REGION, CLIENTAPP_ID);
    }

    internal async Task<bool> SignUpUser(string username, string password, string email, string phonenumber)
    {
        AmazonCognitoIdentityProviderClient provider =
               new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials());

        SignUpRequest signUpRequest = new SignUpRequest();

        signUpRequest.ClientId = CLIENTAPP_ID;
        signUpRequest.Username = username;
        signUpRequest.Password = password;


        AttributeType attributeType = new AttributeType();
        attributeType.Name = "phone_number";
        attributeType.Value = phonenumber;
        signUpRequest.UserAttributes.Add(attributeType);

        AttributeType attributeType1 = new AttributeType();
        attributeType1.Name = "email";
        attributeType1.Value = email;
        signUpRequest.UserAttributes.Add(attributeType1);


        try
        {

            SignUpResponse result = await provider.SignUpAsync(signUpRequest);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;

    }
    internal async Task<bool> VerifyAccessCode(string username, string code)
    {
        AmazonCognitoIdentityProviderClient provider =
               new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials());
        ConfirmSignUpRequest confirmSignUpRequest = new ConfirmSignUpRequest();
        confirmSignUpRequest.Username = username;
        confirmSignUpRequest.ConfirmationCode = code;
        confirmSignUpRequest.ClientId = CLIENTAPP_ID;
        try
        {
            ConfirmSignUpResponse confirmSignUpResult = provider.ConfirmSignUp(confirmSignUpRequest);
            Console.WriteLine(confirmSignUpResult.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }

        return true;

    }
    internal async Task<CognitoUser> ResetPassword(string username)
    {
        AmazonCognitoIdentityProviderClient provider =
               new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials());

        CognitoUserPool userPool = new CognitoUserPool(this.POOL_ID, this.CLIENTAPP_ID, provider);

        CognitoUser user = new CognitoUser(username, this.CLIENTAPP_ID, userPool, provider);
        await user.ForgotPasswordAsync();
        return user;
    }

    internal async Task<CognitoUser> UpdatePassword(string username, string code, string newpassword)
    {
        AmazonCognitoIdentityProviderClient provider =
               new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials());

        CognitoUserPool userPool = new CognitoUserPool(this.POOL_ID, this.CLIENTAPP_ID, provider);

        CognitoUser user = new CognitoUser(username, this.CLIENTAPP_ID, userPool, provider);

        await user.ConfirmForgotPasswordAsync(code, newpassword);
        return user;
    }



    internal async Task<CognitoUser> ValidateUser(string username, string password)
    {
        AmazonCognitoIdentityProviderClient provider =
                new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials());

        CognitoUserPool userPool = new CognitoUserPool(this.POOL_ID, this.CLIENTAPP_ID, provider);

        CognitoUser user = new CognitoUser(username, this.CLIENTAPP_ID, userPool, provider);
        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = password
        };


        AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
        if (authResponse.AuthenticationResult != null)
        {
            return user;
        }
        else
        {
            return null;
        }
    }


    public async Task<CognitoAWSCredentials> ShowCredentialAsync(CognitoUser user)
    {
        CognitoAWSCredentials credentials =
           user.GetCognitoAWSCredentials(FED_POOL_ID, new AppConfigAWSRegion().Region);
        return credentials;
    }

    public async Task<string> CallLambdaAsync(CognitoAWSCredentials credentials, string function, object pl)
    {
        string result = string.Empty;
        using (var client = new AmazonLambdaClient(credentials))
        {
            var _request = new Amazon.Lambda.Model.InvokeRequest { FunctionName = function, Payload = Newtonsoft.Json.JsonConvert.SerializeObject(pl) };
            Amazon.Lambda.Model.InvokeResponse _response = client.Invoke(_request);

            MemoryStream payload = _response.Payload;
            int statusCode = _response.StatusCode;
            StreamReader reader = new StreamReader(payload);
            result = reader.ReadToEnd();
        }
        return result;
    }

}
