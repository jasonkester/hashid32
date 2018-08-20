# HashID32
Generates human-readable, human-transmittable hashes for integers, with low risk of accidental profanity.

These are useful to avoid exposing database ids to end users.

This is a combination of the Hashids algorithm [1] and Douglas Crockford's Base32 Encoding scheme [2].
It borrows implentation details from the .NET version of the former [2].

[1] http://hashids.org/
[2] http://www.crockford.com/wrmg/base32.html
[3] https://github.com/ullmark/hashids.net
