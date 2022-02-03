using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

 
public static class disruptiveAccessToken
    {
        public static string access_token { get; set; }
        public static string token_type { get; set; }
        public static int expires_in { get; set; }
    }


public class initDisruptive : MonoBehaviour
{

     

    //authentication details used in the OAuth2 flow
    private const string KEY_ID         = "YOUR_KEY";
    private const string SECRET_ID     = "YOUR_SECRET";
    private const string EMAIL       = "YOUR_EMAIL";

    

    public class disruptiveaccesstoken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
    

    void Awake()
    {
    StartCoroutine(getDisruptiveData("https://api.disruptive-technologies.com/v2/projects")); //Unity method to get distruptive technology accessToken 
    //getAccessTokenCsharp(KEY_ID ,EMAIL ,SECRET_ID ,"https://identity.disruptive-technologies.com/oauth2/token");     //PurecSharp method to get disruptive technology accessToken
        
    }



    IEnumerator getDisruptiveData(string uri)
    {
        //headers
        
         
      
        yield return getAccessToken(KEY_ID, EMAIL, SECRET_ID);

        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        webRequest.SetRequestHeader("Authorization","Bearer " + disruptiveAccessToken.access_token);

        yield return webRequest.SendWebRequest();

        string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
           

            
    }


    IEnumerator getAccessToken(string key, string email, string secret){

    
    //Set up the signing credentials
    var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(secret));
    var signingCredentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256Signature);
  

    //Construct the JWT header
    var jwt_header = new JwtHeader(signingCredentials);
    jwt_header.Clear();
    jwt_header.Add("alg", "HS256");
    jwt_header.Add("kid", key);

    //Construct the JWT payload
    var jwt_payload = new JwtPayload(
            email, 
            "https://identity.disruptive-technologies.com/oauth2/token",
            null,
            null,
            DateTime.Now.AddHours(1),
            DateTime.Now);


    //Write token to string
    var jwt = new JwtSecurityToken(jwt_header, jwt_payload);   
    var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);   



    //Prepare HTTP POST request data
    Dictionary<string, string> request_data = new Dictionary<string, string>();
    request_data.Add("assertion", tokenString); 
    request_data.Add("grant_type","urn:ietf:params:oauth:grant-type:jwt-bearer");
    
    
 

    //Post request to get access-token
    UnityWebRequest www = UnityWebRequest.Post("https://identity.disruptive-technologies.com/oauth2/token", request_data);
    www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    

    //Send request
    yield return www.SendWebRequest();

    switch (www.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("connection Error during auth" + www.error + www.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error  during auth:  " + www.error + www.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error  during auth: " + www.error + www.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.Success:
                    string resultContent = www.downloadHandler.text;
                    var data = JsonConvert.DeserializeObject<disruptiveaccesstoken>(resultContent);
                    disruptiveAccessToken.access_token = data.access_token;
                    disruptiveAccessToken.expires_in = data.expires_in;
                    disruptiveAccessToken.token_type = data.token_type;
                    
                    // Debug.Log("Received  during auth: " + www.downloadHandler.text);
                    //Debug.Log("ACCESS TOKEN:" + disruptiveAccessToken.access_token);
                    break;
            }
   
        
    }


    async void getAccessTokenCsharp(string key, string email, string secret,string url){
 

    
    //Set up the signing credentials
    var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(secret));
    var signingCredentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256Signature);
  

    //Construct the JWT header
    var jwt_header = new JwtHeader(signingCredentials);
    jwt_header.Clear();
    jwt_header.Add("alg", "HS256");
    jwt_header.Add("kid", key);

    //Construct the JWT payload
    var jwt_payload = new JwtPayload(
            email, 
            "https://identity.disruptive-technologies.com/oauth2/token",
            null,
            null,
            DateTime.Now.AddHours(1),
            DateTime.Now);


    //Write token to string
    var jwt = new JwtSecurityToken(jwt_header, jwt_payload);   
    var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);   
     
    //preparing POST request
    var values = new Dictionary<string, string>
    {
        {"assertion", tokenString},
        {"grant_type","urn:ietf:params:oauth:grant-type:jwt-bearer"}
    };

    var data = new FormUrlEncodedContent(values);
 
    using var client = new HttpClient();
     

    var response = await client.PostAsync(url, data);

    string result = response.Content.ReadAsStringAsync().Result;

    }



}
