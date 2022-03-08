#if !NETFX_CORE

using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace CLARTE.Net.HTTP
{
	public class Client
	{
		private const string username = "toto";
		private const string password = "password";

		private byte[] certificate;

		public Client(string certificate_filename)
		{
			Uri url = new Uri("http://localhost:8080/index.html");

			LoadCertificate(certificate_filename);

			ServicePointManager.ServerCertificateValidationCallback += Validator;

			WebClient client = new WebClient();

			client.Credentials = new NetworkCredential(username, password);

			client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
				Debug.LogError("Downloaded " + (100.0 * (double) e.BytesReceived / (double) e.TotalBytesToReceive) + "% (" + e.BytesReceived + " / " + e.TotalBytesToReceive + " bytes).");
			};

			client.DownloadFileCompleted += (sender, e) =>
			{
				if(e.Cancelled)
				{
					Debug.LogErrorFormat("Download of '{0}' canceled", url);
				}
				else if(e.Error != null)
				{
					Debug.LogErrorFormat("Error during download of '{0}': {1}", url, e.Error.Message);
				}
			};

			client.DownloadFileAsync(url, Path.GetFileName(url.LocalPath));
		}

		private void LoadCertificate(string filename)
		{
			try
			{
				if(File.Exists(filename))
				{
					certificate = File.ReadAllBytes(filename);
				}
				else
				{
					Debug.LogErrorFormat("Impossible to find certificate '{0}'", filename);
				}
			}
			catch(Exception exception)
			{
				Debug.LogError(exception);
			}
		}

		private bool Validator(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors ssl_policy_errors)
		{
			byte[] server_certificate = cert.GetRawCertData();

			bool match = (server_certificate.Length == certificate.Length) &&
				DateTime.Parse(cert.GetEffectiveDateString()) <= DateTime.Now &&
				DateTime.Parse(cert.GetExpirationDateString()) >= DateTime.Now;

			int size = server_certificate.Length;

			int i = 0;

			while(match && i < size)
			{
				match = server_certificate[i] == certificate[i];

				++i;
			}

			return match;
		}
	}
}

#endif // !NETFX_CORE
