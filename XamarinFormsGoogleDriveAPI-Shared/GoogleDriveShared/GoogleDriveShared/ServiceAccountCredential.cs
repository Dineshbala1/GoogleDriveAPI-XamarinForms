using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Json;
using Google.Apis.Util;
using Google.Apis.Auth.OAuth2;
using RsaKey = System.Security.Cryptography.RSACryptoServiceProvider;
using Google.Apis.Auth;

namespace XamarinFormsGoogleDriveAPI
{
    public class ServiceAccountCredential : ServiceCredential
    {
        private const string Sha256Oid = "2.16.840.1.101.3.4.2.1";
        /// <summary>An initializer class for the service account credential. </summary>
        new public class Initializer : ServiceCredential.Initializer
        {
            /// <summary>Gets the service account ID (typically an e-mail address).</summary>
            public string Id { get; private set; }

            /// <summary>
            /// Gets or sets the email address of the user the application is trying to impersonate in the service 
            /// account flow or <c>null</c>.
            /// </summary>
            public string User { get; set; }

            /// <summary>Gets the scopes which indicate API access your application is requesting.</summary>
            public IEnumerable<string> Scopes { get; set; }

            /// <summary>
            /// Gets or sets the key which is used to sign the request, as specified in
            /// https://developers.google.com/accounts/docs/OAuth2ServiceAccount#computingsignature.
            /// </summary>
            public RsaKey Key { get; set; }

            /// <summary>Constructs a new initializer using the given id.</summary>
            public Initializer(string id)
                : this(id, GoogleAuthConsts.TokenUrl) { }

            /// <summary>Constructs a new initializer using the given id and the token server URL.</summary>
            public Initializer(string id, string tokenServerUrl) : base(tokenServerUrl)
            {
                Id = id;
                Scopes = new List<string>();
            }

            /// <summary>Extracts the <see cref="Key"/> from the given PKCS8 private key.</summary>
            public Initializer FromPrivateKey(string privateKey)
            {
                RSAParameters rsaParameters = Pkcs8.DecodeRsaParameters(privateKey);
                Key = (RsaKey)RSA.Create();
                Key.ImportParameters(rsaParameters);
                return this;
            }

            /// <summary>Extracts a <see cref="Key"/> from the given certificate.</summary>
            public Initializer FromCertificate(X509Certificate2 certificate)
            {
#if NETSTANDARD
                Key = certificate.GetRSAPrivateKey();
#else
                // Workaround to correctly cast the private key as a RSACryptoServiceProvider type 24.
                RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
                byte[] privateKeyBlob = rsa.ExportCspBlob(true);
                Key = new RSACryptoServiceProvider();
                Key.ImportCspBlob(privateKeyBlob);
#endif
                return this;
            }
        }

        /// <summary>Unix epoch as a <c>DateTime</c></summary>
        protected static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly string id;
        private readonly string user;
        private readonly IEnumerable<string> scopes;
        private readonly RsaKey key;

        /// <summary>Gets the service account ID (typically an e-mail address).</summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Gets the email address of the user the application is trying to impersonate in the service account flow 
        /// or <c>null</c>.
        /// </summary>
        public string User { get { return user; } }

        /// <summary>Gets the service account scopes.</summary>
        public IEnumerable<string> Scopes { get { return scopes; } }

        /// <summary>
        /// Gets the key which is used to sign the request, as specified in
        /// https://developers.google.com/accounts/docs/OAuth2ServiceAccount#computingsignature.
        /// </summary>
        public RsaKey Key { get { return key; } }

        /// <summary><c>true</c> if this credential has any scopes associated with it.</summary>
        internal bool HasScopes { get { return scopes != null && scopes.Any(); } }

        /// <summary>Constructs a new service account credential using the given initializer.</summary>
        public ServiceAccountCredential(Initializer initializer) : base(initializer)
        {
            id = initializer.Id.ThrowIfNullOrEmpty("initializer.Id");
            user = initializer.User;
            scopes = initializer.Scopes;
            key = initializer.Key.ThrowIfNull("initializer.Key");
        }

        /// <summary>
        /// Creates a new <see cref="ServiceAccountCredential"/> instance from JSON credential data.
        /// </summary>
        /// <param name="credentialData">The stream from which to read the JSON key data for a service account. Must not be null.</param>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="credentialData"/> does not contain valid JSON service account key data.
        /// </exception>
        /// <returns>The credentials parsed from the service account key data.</returns>
        public static ServiceAccountCredential FromServiceAccountData(Stream credentialData)
        {
            var credential = GoogleCredential.FromStream(credentialData);
            var result = credential;
            if (result == null)
            {
                throw new InvalidOperationException("JSON data does not represent a valid service account credential.");
            }
            return new ServiceAccountCredential(new Initializer(""));
        }

        /// <summary>
        /// Requests a new token as specified in 
        /// https://developers.google.com/accounts/docs/OAuth2ServiceAccount#makingrequest.
        /// </summary>
        /// <param name="taskCancellationToken">Cancellation token to cancel operation.</param>
        /// <returns><c>true</c> if a new token was received successfully.</returns>
        public override async Task<bool> RequestAccessTokenAsync(CancellationToken taskCancellationToken)
        {
            // Create the request.
            var request = new GoogleAssertionTokenRequest()
            {
                Assertion = CreateAssertionFromPayload(CreatePayload())
            };

            Logger.Debug("Request a new access token. Assertion data is: " + request.Assertion);

            var newToken = await request.ExecuteAsync(HttpClient, TokenServerUrl, taskCancellationToken, Clock)
                .ConfigureAwait(false);
            Token = newToken;
            return true;
        }

        /// <summary>
        /// Gets an access token to authorize a request.
        /// If <paramref name="authUri"/> is set and this credential has no scopes associated
        /// with it, a locally signed JWT access token for given <paramref name="authUri"/>
        /// is returned. Otherwise, an OAuth2 access token obtained from token server will be returned.
        /// A cached token is used if possible and the token is only refreshed once it's close to its expiry.
        /// </summary>
        /// <param name="authUri">The URI the returned token will grant access to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The access token.</returns>
        public override async Task<string> GetAccessTokenForRequestAsync(string authUri = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!HasScopes && authUri != null)
            {
                // TODO(jtattermusch): support caching of JWT access tokens per authUri, currently a new 
                // JWT access token is created each time, which can hurt performance.
                return CreateJwtAccessToken(authUri);
            }
            return await base.GetAccessTokenForRequestAsync(authUri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a JWT access token than can be used in request headers instead of an OAuth2 token.
        /// This is achieved by signing a special JWT using this service account's private key.
        /// <param name="authUri">The URI for which the access token will be valid.</param>
        /// </summary>
        private string CreateJwtAccessToken(string authUri)
        {
            var issuedDateTime = DateTime.UtcNow;
            var issued = (int)(issuedDateTime - UnixEpoch).TotalSeconds;
            var payload = new JsonWebSignature.Payload()
            {
                Issuer = Id,
                Subject = Id,
                Audience = authUri,
                IssuedAtTimeSeconds = issued,
                ExpirationTimeSeconds = issued + 3600,
            };

            return CreateAssertionFromPayload(payload);
        }

        /// <summary>
        /// Signs JWT token using the private key and returns the serialized assertion.
        /// </summary>
        /// <param name="payload">the JWT payload to sign.</param>
        private string CreateAssertionFromPayload(JsonWebSignature.Payload payload)
        {
            string serializedHeader = CreateSerializedHeader();
            string serializedPayload = NewtonsoftJsonSerializer.Instance.Serialize(payload);

            var assertion = new StringBuilder();
            assertion.Append(UrlSafeBase64Encode(serializedHeader))
                .Append('.')
                .Append(UrlSafeBase64Encode(serializedPayload));
            var signature = CreateSignature(Encoding.ASCII.GetBytes(assertion.ToString()));
            assertion.Append('.').Append(UrlSafeEncode(signature));
            return assertion.ToString();
        }

        /// <summary>
        /// Creates a base64 encoded signature for the SHA-256 hash of the specified data.
        /// </summary>
        /// <param name="data">The data to hash and sign. Must not be null.</param>
        /// <returns>The base-64 encoded signature.</returns>
        public string CreateSignature(byte[] data)
        {
            data.ThrowIfNull(nameof(data));

            using (var hashAlg = SHA256.Create())
            {
                byte[] assertionHash = hashAlg.ComputeHash(data);
#if NETSTANDARD
                var sigBytes = key.SignHash(assertionHash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#else
                var sigBytes = key.SignHash(assertionHash, Sha256Oid);
#endif
                return Convert.ToBase64String(sigBytes);
            }
        }

        /// <summary>
        /// Creates a serialized header as specified in 
        /// https://developers.google.com/accounts/docs/OAuth2ServiceAccount#formingheader.
        /// </summary>
        private static string CreateSerializedHeader()
        {
            var header = new GoogleJsonWebSignature.Header()
            {
                Algorithm = "RS256",
                Type = "JWT"
            };

            return NewtonsoftJsonSerializer.Instance.Serialize(header);
        }

        /// <summary>
        /// Creates a claim set as specified in 
        /// https://developers.google.com/accounts/docs/OAuth2ServiceAccount#formingclaimset.
        /// </summary>
        private GoogleJsonWebSignature.Payload CreatePayload()
        {
            var issued = (int)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
            return new GoogleJsonWebSignature.Payload()
            {
                Issuer = Id,
                Audience = TokenServerUrl,
                IssuedAtTimeSeconds = issued,
                ExpirationTimeSeconds = issued + 3600,
                Subject = User,
                Scope = String.Join(" ", Scopes)
            };
        }

        /// <summary>Encodes the provided UTF8 string into an URL safe base64 string.</summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>The URL safe base64 string.</returns>
        private string UrlSafeBase64Encode(string value)
        {
            return UrlSafeBase64Encode(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>Encodes the byte array into an URL safe base64 string.</summary>
        /// <param name="bytes">Byte array to encode.</param>
        /// <returns>The URL safe base64 string.</returns>
        private string UrlSafeBase64Encode(byte[] bytes)
        {
            return UrlSafeEncode(Convert.ToBase64String(bytes));
        }

        /// <summary>Encodes the base64 string into an URL safe string.</summary>
        /// <param name="base64Value">The base64 string to make URL safe.</param>
        /// <returns>The URL safe base64 string.</returns>
        private string UrlSafeEncode(string base64Value)
        {
            return base64Value.Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
        }
    }

    internal class Pkcs8
    {
        // PKCS#8 specification: https://www.ietf.org/rfc/rfc5208.txt
        // ASN.1 specification: https://www.itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf

        /// <summary>
        /// An incomplete ASN.1 decoder, only implements what's required
        /// to decode a Service Credential.
        /// </summary>
        internal class Asn1
        {
            internal enum Tag
            {
                Integer = 2,
                OctetString = 4,
                Null = 5,
                ObjectIdentifier = 6,
                Sequence = 16,
            }

            internal class Decoder
            {
                public Decoder(byte[] bytes)
                {
                    _bytes = bytes;
                    _index = 0;
                }

                private byte[] _bytes;
                private int _index;

                public object Decode()
                {
                    Tag tag = ReadTag();
                    switch (tag)
                    {
                        case Tag.Integer:
                            return ReadInteger();
                        case Tag.OctetString:
                            return ReadOctetString();
                        case Tag.Null:
                            return ReadNull();
                        case Tag.ObjectIdentifier:
                            return ReadOid();
                        case Tag.Sequence:
                            return ReadSequence();
                        default:
                            throw new NotSupportedException($"Tag '{tag}' not supported.");
                    }
                }

                private byte NextByte() => _bytes[_index++];

                private byte[] ReadLengthPrefixedBytes()
                {
                    int length = ReadLength();
                    return ReadBytes(length);
                }

                private byte[] ReadInteger() => ReadLengthPrefixedBytes();

                private object ReadOctetString()
                {
                    byte[] bytes = ReadLengthPrefixedBytes();
                    return new Decoder(bytes).Decode();
                }

                private object ReadNull()
                {
                    int length = ReadLength();
                    if (length != 0)
                    {
                        throw new InvalidDataException("Invalid data, Null length must be 0.");
                    }
                    return null;
                }

                private int[] ReadOid()
                {
                    byte[] oidBytes = ReadLengthPrefixedBytes();
                    List<int> result = new List<int>();
                    bool first = true;
                    int index = 0;
                    while (index < oidBytes.Length)
                    {
                        int subId = 0;
                        byte b;
                        do
                        {
                            b = oidBytes[index++];
                            if ((subId & 0xff000000) != 0)
                            {
                                throw new NotSupportedException("Oid subId > 2^31 not supported.");
                            }
                            subId = (subId << 7) | (b & 0x7f);
                        } while ((b & 0x80) != 0);
                        if (first)
                        {
                            first = false;
                            result.Add(subId / 40);
                            result.Add(subId % 40);
                        }
                        else
                        {
                            result.Add(subId);
                        }
                    }
                    return result.ToArray();
                }

                private object[] ReadSequence()
                {
                    int length = ReadLength();
                    int endOffset = _index + length;
                    if (endOffset < 0 || endOffset > _bytes.Length)
                    {
                        throw new InvalidDataException("Invalid sequence, too long.");
                    }
                    List<object> sequence = new List<object>();
                    while (_index < endOffset)
                    {
                        sequence.Add(Decode());
                    }
                    return sequence.ToArray();
                }

                private byte[] ReadBytes(int length)
                {
                    if (length <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length), "length must be positive.");
                    }
                    if (_bytes.Length - length < 0)
                    {
                        throw new ArgumentException("Cannot read past end of buffer.");
                    }
                    byte[] result = new byte[length];
                    Array.Copy(_bytes, _index, result, 0, length);
                    _index += length;
                    return result;
                }

                private Tag ReadTag()
                {
                    byte b = NextByte();
                    int tag = b & 0x1f;
                    if (tag == 0x1f)
                    {
                        // A tag value of 0x1f (31) indicates a tag value of >30 (spec section 8.1.2.4)
                        throw new NotSupportedException("Tags of value > 30 not supported.");
                    }
                    else
                    {
                        return (Tag)tag;
                    }
                }

                private int ReadLength()
                {
                    byte b0 = NextByte();
                    if ((b0 & 0x80) == 0)
                    {
                        return b0;
                    }
                    else
                    {
                        if (b0 == 0xff)
                        {
                            throw new InvalidDataException("Invalid length byte: 0xff");
                        }
                        int byteCount = b0 & 0x7f;
                        if (byteCount == 0)
                        {
                            throw new NotSupportedException("Lengths in Indefinite Form not supported.");
                        }
                        int result = 0;
                        for (int i = 0; i < byteCount; i++)
                        {
                            if ((result & 0xff800000) != 0)
                            {
                                throw new NotSupportedException("Lengths > 2^31 not supported.");
                            }
                            result = (result << 8) | NextByte();
                        }
                        return result;
                    }
                }

            }

            public static object Decode(byte[] bs) => new Decoder(bs).Decode();

        }

        public static RSAParameters DecodeRsaParameters(string pkcs8PrivateKey)
        {
            const string PrivateKeyPrefix = "-----BEGIN PRIVATE KEY-----";
            const string PrivateKeySuffix = "-----END PRIVATE KEY-----";

            Utilities.ThrowIfNullOrEmpty(pkcs8PrivateKey, nameof(pkcs8PrivateKey));
            pkcs8PrivateKey = pkcs8PrivateKey.Trim();
            if (!pkcs8PrivateKey.StartsWith(PrivateKeyPrefix) || !pkcs8PrivateKey.EndsWith(PrivateKeySuffix))
            {
                throw new ArgumentException(
                    $"PKCS8 data must be contained within '{PrivateKeyPrefix}' and '{PrivateKeySuffix}'.", nameof(pkcs8PrivateKey));
            }
            string base64PrivateKey =
                pkcs8PrivateKey.Substring(PrivateKeyPrefix.Length, pkcs8PrivateKey.Length - PrivateKeyPrefix.Length - PrivateKeySuffix.Length);
            // FromBase64String() ignores whitespace, so further Trim()ing isn't required.
            byte[] pkcs8Bytes = Convert.FromBase64String(base64PrivateKey);

            object ans1 = Asn1.Decode(pkcs8Bytes);
            object[] parameters = (object[])((object[])ans1)[2];

            var rsaParmeters = new RSAParameters
            {
                Modulus = TrimLeadingZeroes((byte[])parameters[1]),
                Exponent = TrimLeadingZeroes((byte[])parameters[2]),
                D = TrimLeadingZeroes((byte[])parameters[3]),
                P = TrimLeadingZeroes((byte[])parameters[4]),
                Q = TrimLeadingZeroes((byte[])parameters[5]),
                DP = TrimLeadingZeroes((byte[])parameters[6]),
                DQ = TrimLeadingZeroes((byte[])parameters[7]),
                InverseQ = TrimLeadingZeroes((byte[])parameters[8]),
            };

            return rsaParmeters;
        }

        private static byte[] TrimLeadingZeroes(byte[] bs)
        {
            int zeroCount = 0;
            while (zeroCount < bs.Length && bs[zeroCount] == 0) zeroCount += 1;
            if (zeroCount == 0)
            {
                return bs;
            }
            else
            {
                byte[] result = new byte[bs.Length - zeroCount];
                Array.Copy(bs, zeroCount, result, 0, result.Length);
                return result;
            }
        }

    }
}