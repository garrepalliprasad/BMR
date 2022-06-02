using BMR.Common.Models;
using BMR.Common.SEALUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Research.SEAL;
using System;
using System.Collections.Generic;

namespace BMR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BmrapiController : ControllerBase
    {
        Utilities utilities = new Utilities();
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return Ok("Welcome to BMR API");
        }
        [HttpPost]
        [Route("")]
        public IActionResult CalculateBmr(BmrencodedData data)
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
            double w = 13.397, h = 4.799, a = 5.677, c = 88.362, d = 1;
            Plaintext wPlain = new Plaintext();
            Plaintext hPlain = new Plaintext();
            Plaintext aPlain = new Plaintext();
            Plaintext cPlain = new Plaintext();
            Plaintext dPlain = new Plaintext();
            encoder.Encode(w, scale, wPlain);
            encoder.Encode(h, scale, hPlain);
            encoder.Encode(a, scale, aPlain);
            encoder.Encode(c, scale, cPlain);
            encoder.Encode(d, scale, dPlain);
            Ciphertext wCiphertext = new Ciphertext();
            Ciphertext hCiphertext = new Ciphertext();
            Ciphertext aCiphertext = new Ciphertext();
            Ciphertext cCiphertext = new Ciphertext();
            Ciphertext dCiphertext = new Ciphertext();
            encryptor.Encrypt(wPlain, wCiphertext);
            encryptor.Encrypt(hPlain, hCiphertext);
            encryptor.Encrypt(aPlain, aCiphertext);
            encryptor.Encrypt(cPlain, cCiphertext);
            encryptor.Encrypt(dPlain, dCiphertext);
            Ciphertext dataWeight = utilities.BuildCiphertextFromBase64String(data.Weight, context);
            Ciphertext dataHeight = utilities.BuildCiphertextFromBase64String(data.Height, context);
            Ciphertext dataAge = utilities.BuildCiphertextFromBase64String(data.Age, context);
            Ciphertext bmrResult = new Ciphertext();
            Ciphertext wtempResult = new Ciphertext();
            Ciphertext htempResult = new Ciphertext();
            Ciphertext atempResult = new Ciphertext();
            Ciphertext cdtempResult = new Ciphertext();
            evaluator.Multiply(wCiphertext, dataWeight, wtempResult);
            evaluator.Multiply(hCiphertext, dataHeight, htempResult);
            evaluator.Multiply(aCiphertext, dataAge, atempResult);
            evaluator.Multiply(cCiphertext, dCiphertext, cdtempResult);
            evaluator.Add(wtempResult, htempResult, bmrResult);
            evaluator.Sub(bmrResult, atempResult, bmrResult);
            evaluator.Add(bmrResult, cdtempResult, bmrResult);

            Plaintext bmrPlain = new Plaintext();
            List<double> bmr = new List<double>();
            decryptor.Decrypt(bmrResult, bmrPlain);
            encoder.Decode(bmrPlain, bmr);

            data.BMRResult = utilities.CiphertextToBase64String(bmrResult);
            return Ok(data);

            {
                //double scale = Math.Pow(2.0, 40);
                //double w = 13.397, h = 4.799, a = 5.677, c = 88.362, weight = 70, height = 180, age = 38, d = 1;
                //Plaintext wPlain = new Plaintext();
                //Plaintext hPlain = new Plaintext();
                //Plaintext aPlain = new Plaintext();
                //Plaintext cPlain = new Plaintext();
                //Plaintext dPlain = new Plaintext();
                ////Plaintext weightPlain = new Plaintext();
                ////Plaintext heightPlain = new Plaintext();
                ////Plaintext agePlain = new Plaintext();

                //utilities.CKKSEncoder.Encode(w, scale, wPlain);
                //utilities.CKKSEncoder.Encode(h, scale, hPlain);
                //utilities.CKKSEncoder.Encode(a, scale, aPlain);
                //utilities.CKKSEncoder.Encode(c, scale, cPlain);
                //utilities.CKKSEncoder.Encode(d, scale, dPlain);
                ////utilities.CKKSEncoder.Encode(weight, scale, weightPlain);
                ////utilities.CKKSEncoder.Encode(height, scale, heightPlain);
                ////utilities.CKKSEncoder.Encode(age, scale, agePlain);

                //Ciphertext wCiphertext = new Ciphertext();
                //Ciphertext hCiphertext = new Ciphertext();
                //Ciphertext aCiphertext = new Ciphertext();
                //Ciphertext cCiphertext = new Ciphertext();
                //Ciphertext dCiphertext = new Ciphertext();
                ////Ciphertext weightCiphertext = new Ciphertext();
                ////Ciphertext heightCiphertext = new Ciphertext();
                ////Ciphertext ageCiphertext = new Ciphertext();
                //Ciphertext weightCiphertext = utilities.BuildCiphertextFromBase64String(bmrencodedData.Weight, context);
                //Ciphertext heightCiphertext = utilities.BuildCiphertextFromBase64String(bmrencodedData.Height, context);
                //Ciphertext ageCiphertext = utilities.BuildCiphertextFromBase64String(bmrencodedData.Age, context);

                //utilities.Encryptor.Encrypt(wPlain, wCiphertext);
                //utilities.Encryptor.Encrypt(hPlain, hCiphertext);
                //utilities.Encryptor.Encrypt(aPlain, aCiphertext);
                //utilities.Encryptor.Encrypt(cPlain, cCiphertext);
                //utilities.Encryptor.Encrypt(dPlain, dCiphertext);
                ////utilities.Encryptor.Encrypt(weightPlain, weightCiphertext);
                ////utilities.Encryptor.Encrypt(heightPlain, heightCiphertext);
                ////utilities.Encryptor.Encrypt(agePlain, ageCiphertext);

                ////BMR = 13.397W + 4.799H - 5.677A + 88.362
                ////BMR = 9.247W + 3.098H - 4.330A + 447.593

                //Ciphertext bmrResult = new Ciphertext();
                //Ciphertext wtempResult = new Ciphertext();
                //Ciphertext htempResult = new Ciphertext();
                //Ciphertext atempResult = new Ciphertext();
                //Ciphertext cdtempResult = new Ciphertext();
                //utilities.Evaluator.Multiply(wCiphertext, weightCiphertext, wtempResult);
                //utilities.Evaluator.Multiply(hCiphertext, heightCiphertext, htempResult);
                //utilities.Evaluator.Multiply(aCiphertext, ageCiphertext, atempResult);
                //utilities.Evaluator.Multiply(cCiphertext, dCiphertext, cdtempResult);
                //utilities.Evaluator.Add(wtempResult, htempResult, bmrResult);
                //utilities.Evaluator.Sub(bmrResult, atempResult, bmrResult);
                //utilities.Evaluator.Add(bmrResult, cdtempResult, bmrResult);
                //Plaintext bmrPlain = new Plaintext();
                //List<double> bmr = new List<double>();

                //utilities.Decryptor.Decrypt(bmrResult, bmrPlain);
                //utilities.CKKSEncoder.Decode(bmrPlain, bmr);
                //string Result = utilities.CiphertextToBase64String(bmrResult);
                //Bmrencodedresult bmrencodedresult = new Bmrencodedresult() { BMRResult = Result };
                //return Ok(bmrencodedresult);
            }
        }
    }
}
