using BMR.Common;
using BMR.Common.Models;
using BMR.Common.SEALUtilities;
using Calculator.WebAPP.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Research.SEAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BMR.Ui.Controllers
{
    
    public class BmruiController : Controller
    {
        CalculatorAPI _api = new CalculatorAPI();
        Utilities utilities = new Utilities();
        HttpClient client;

        public BmruiController()
        {
            client = _api.Initial();
        }
        [HttpGet]
        public IActionResult Index()
        {
            UserData userData = new UserData();
            return View(userData);
        }
        [HttpPost]
        public async Task<IActionResult> Index(UserData data)
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            ulong polyModulusDegree = 8192;
            parms.PolyModulusDegree = polyModulusDegree;
            parms.CoeffModulus = CoeffModulus.Create(polyModulusDegree, new int[] { 60, 40, 40, 60 });
            double scale = Math.Pow(2.0, 40);
            SEALContext context = new SEALContext(parms);
            KeyGenerator keygen = new KeyGenerator(context);
            SecretKey secretKey = keygen.SecretKey;
            keygen.CreatePublicKey(out PublicKey publicKey);
            Encryptor encryptor = new Encryptor(context, publicKey);
            Evaluator evaluator = new Evaluator(context);
            Decryptor decryptor = new Decryptor(context, secretKey);
            CKKSEncoder encoder = new CKKSEncoder(context);
            double weight=data.Weight, height=data.Height, age=data.Age;
            Plaintext weightPlain = new Plaintext();
            Plaintext heightPlain = new Plaintext();
            Plaintext agePlain = new Plaintext();
            encoder.Encode(weight, scale, weightPlain);
            encoder.Encode(height, scale, heightPlain);
            encoder.Encode(age, scale, agePlain);
            Ciphertext weightCiphertext = new Ciphertext();
            Ciphertext heightCiphertext = new Ciphertext();
            Ciphertext ageCiphertext = new Ciphertext();
            encryptor.Encrypt(weightPlain, weightCiphertext);
            encryptor.Encrypt(heightPlain, heightCiphertext);
            encryptor.Encrypt(agePlain, ageCiphertext);
            BmrencodedData bmrencodedData = new BmrencodedData()
            {
                Weight = utilities.CiphertextToBase64String(weightCiphertext),
                Height = utilities.CiphertextToBase64String(heightCiphertext),
                Age = utilities.CiphertextToBase64String(ageCiphertext)
            };
            StringContent content = new StringContent(JsonConvert.SerializeObject(bmrencodedData), Encoding.UTF8, "application/json");
            HttpResponseMessage res = await client.PostAsync("api/bmrapi",content);
            if (res.IsSuccessStatusCode)
            {
                BmrencodedData encodedData = JsonConvert.DeserializeObject<BmrencodedData>(await res.Content.ReadAsStringAsync());
                Ciphertext bmrResult=utilities.BuildCiphertextFromBase64String(encodedData.BMRResult,context);
                Plaintext bmrPlain = new Plaintext();
                List<double> bmr = new List<double>();

                decryptor.Decrypt(bmrResult, bmrPlain);
                encoder.Decode(bmrPlain, bmr);
            }
            return View();
    
            {
            //    Plaintext heightPlain = new Plaintext();
            //    Plaintext weightPlain = new Plaintext();
            //    Plaintext agePlain = new Plaintext();
            //    double scale = Math.Pow(2.0, 40);
            //    utilities.CKKSEncoder.Encode(data.Height, scale, heightPlain);
            //    utilities.CKKSEncoder.Encode(data.Weight, scale, weightPlain);
            //    utilities.CKKSEncoder.Encode(data.Age, scale, agePlain);
            //    Ciphertext heightEncrypted = new Ciphertext();
            //    Ciphertext weightEncrypted = new Ciphertext();
            //    Ciphertext ageEncrypted = new Ciphertext();
            //    utilities.Encryptor.Encrypt(heightPlain, heightEncrypted);
            //    utilities.Encryptor.Encrypt(weightPlain, weightEncrypted);
            //    utilities.Encryptor.Encrypt(agePlain, ageEncrypted);
            //    BmrencodedData bmrencodedData = new BmrencodedData()
            //    {
            //        Age = utilities.CiphertextToBase64String(ageEncrypted),
            //        Height = utilities.CiphertextToBase64String(heightEncrypted),
            //        Weight = utilities.CiphertextToBase64String(weightEncrypted),
            //        Gender = data.Gender
            //    };
            //    HttpClient client = _api.Initial();
            //    Ciphertext bmrEncrypted = new Ciphertext();
            //    StringContent content = new StringContent(JsonConvert.SerializeObject(bmrencodedData), Encoding.UTF8, "application/json");
            //    HttpResponseMessage res = await client.PostAsync("api/bmrapi", content);
            //    Bmrencodedresult bmrencodedresult;
            //    if (res.IsSuccessStatusCode)
            //    {
            //        var result = await res.Content.ReadAsStringAsync();
            //        bmrencodedresult = JsonConvert.DeserializeObject<Bmrencodedresult>(result);
            //        bmrEncrypted = utilities.BuildCiphertextFromBase64String(bmrencodedresult.BMRResult, utilities.SEALContext);
            //        Plaintext bmrPlain = new Plaintext();
            //        utilities.Decryptor.Decrypt(bmrEncrypted, bmrPlain);
            //        List<double> bmr = new List<double>();
            //        utilities.CKKSEncoder.Decode(bmrPlain, bmr);
            //        return RedirectToAction("Result", "bmrui", bmr[0]);
            //    }
            //    return View(data);
            }
        }
        [HttpGet]
        public IActionResult Result(double bmr)
        {
            ViewBag.BMR=bmr;
            return View();
        }
    }
}
