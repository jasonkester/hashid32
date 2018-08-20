using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpatSoftware.Helpers
{
	/// <summary>
	/// Generates human-readable, human-transmitable hashes for integers, with low risk of accidental profanity.
	/// These are useful to avoid exposing database ids to end users.
	/// This is a combination of the Hashids algorithm [1] and Douglas Crockford's Base32 Encoding scheme [2].
	/// It borrows implentation details from the .NET version of the former [2].
	/// 
	/// [1] http://hashids.org/
	/// [2] http://www.crockford.com/wrmg/base32.html
	/// [3] https://github.com/ullmark/hashids.net
	/// </summary>
	public class HashID32
	{
		public const string HASH_ALPHABET = "abcdefghjkmnpqrstvwxyz1234567890";
		public const string LOTTERY_ALPHABET = "abcdefghjkmnpqrstvwxyz";
		private static Dictionary<char, char> _inmap;

		private string Alphabet { get; set; }
		private string Lottery { get; set; }
		public string Salt { get; private set; }

		/// <summary>
		/// Instantiates with an empty salt.
		/// </summary>
		public HashID32()
			: this(string.Empty)
		{ }

		/// <summary>
		/// Instantiates with the supplied salt.
		/// </summary>
		/// <param name="salt"></param>
		public HashID32(string salt = "")
		{
			Salt = salt;
			Alphabet = ConsistentShuffle(HASH_ALPHABET, salt);
			Lottery = ConsistentShuffle(LOTTERY_ALPHABET, salt);

			_inmap = new Dictionary<char, char>();
			_inmap['u'] = 'v';
			_inmap['o'] = '0';
			_inmap['i'] = '1';
			_inmap['l'] = '1';
		}

		/// <summary>
		/// Encodes the provided number into a hashed string
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public string Encode(long number)
		{
			var alphabet = string.Copy(Alphabet);
			var lottery = Lottery[(int)number % Lottery.Length];
			var buffer = lottery + Salt + alphabet;

			alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));

			return lottery + this.Hash(number, alphabet);
		}

		/// <summary>
		/// Decodes the provided hash into an int value
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		public long Decode(string hash)
		{
			if (String.IsNullOrEmpty(hash) || String.IsNullOrEmpty(hash.Trim()))
				return 0;

			hash = Transform(hash);

			var alphabet = string.Copy(this.Alphabet);
			var lottery = hash[0];
			var remainder = hash.Substring(1);

			var buffer = lottery + Salt + alphabet;

			alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
			long value = Unhash(remainder, alphabet);

			// Every hash will resolve to a number, so we need to ensure that 
			// this is the hash we would have created for this number.
			if (Encode(value) == hash)
			{
				return value;
			}

			return 0;
		}

		/// <summary>
		/// Takes human input with possible capitalization/interpretation error and
		/// returns a hash that we can feed into our decoder.
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		private string Transform(string hash)
		{
			hash = hash.ToLower();
			foreach (var c in _inmap.Keys)
			{
				hash = hash.Replace(c, _inmap[c]);
			}

			return hash;
		}

		/// <summary>
		/// Detects obviously bad hashes
		/// </summary>
		/// <param name="base32Value"></param>
		/// <returns></returns>
		public static bool IsValid(string base32Value)
		{
			return new Regex("^[abcdefghjkmnpqrstvwxyz1234567890uoil]+$", RegexOptions.Compiled).IsMatch(base32Value.ToLower());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="alphabet"></param>
		/// <returns></returns>
		private string Hash(long input, string alphabet)
		{
			var hash = new StringBuilder();

			do
			{
				hash.Insert(0, alphabet[(int)(input % alphabet.Length)]);
				input = (input / alphabet.Length);
			} while (input > 0);

			return hash.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="alphabet"></param>
		/// <returns></returns>
		private long Unhash(string input, string alphabet)
		{
			long number = 0;

			for (var i = 0; i < input.Length; i++)
			{
				var pos = alphabet.IndexOf(input[i]);
				number += (long)(pos * Math.Pow(alphabet.Length, input.Length - i - 1));
			}

			return number;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="alphabet"></param>
		/// <param name="salt"></param>
		/// <returns></returns>
		private string ConsistentShuffle(string alphabet, string salt)
		{
			if (String.IsNullOrEmpty(salt) || String.IsNullOrEmpty(salt.Trim()))
				return alphabet;

			int v, p, n, j;
			v = p = n = j = 0;

			for (var i = alphabet.Length - 1; i > 0; i--, v++)
			{
				v %= salt.Length;
				p += n = (int)salt[v];
				j = (n + v + p) % i;

				var temp = alphabet[j];
				alphabet = alphabet.Substring(0, j) + alphabet[i] + alphabet.Substring(j + 1);
				alphabet = alphabet.Substring(0, i) + temp + alphabet.Substring(i + 1);
			}

			return alphabet;
		}

	}
}