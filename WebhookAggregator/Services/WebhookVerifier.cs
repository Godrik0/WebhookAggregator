// <copyright file="WebhookVerifier.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WebhookAggregator.Option;

namespace WebhookAggregator.Services;

public class WebhookVerifier : IWebhookVerifier
{
    private readonly string _secret;

    public WebhookVerifier(IOptions<WebhookVerifierOptions> options)
    {
        _secret = options.Value.Secret;
    }

    private const string ShaPrefix = "sha256=";

    public bool IsValid(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature)) return false;

        var expectedHash = ComputeHmacSha256(payload, _secret);
        var actualHash = HexToBytes(signature);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    private static byte[] ComputeHmacSha256(string payload, string key)
    {
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var keyBytes = Encoding.UTF8.GetBytes(key);

        using var hmac = new HMACSHA256(keyBytes);
        hmac.ComputeHash(payloadBytes);
        return hmac.Hash;
    }

    private static byte[] HexToBytes(string hex)
    {
        if (hex.StartsWith(ShaPrefix)) hex = hex[ShaPrefix.Length..];
        return Enumerable.Range(0, hex.Length / 2)
            .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
            .ToArray();
    }

    // private static bool SlowEquals(byte[] a, byte[] b)
    // {
    //     var diff = (uint)a.Length ^ (uint)b.Length;
    //     byte[] c = [0];
    //     for (var i = 0; i < a.Length; i++)
    //         diff |= (uint)(GetElem(a, i, c, 0) ^ GetElem(b, i, c, 0));
    //     return diff == 0;
    // }
    //
    // private static byte GetElem(byte[] x, int i, byte[] c, int i0)
    // {
    //     var ok = (i < x.Length);
    //     return (ok ? x : c)[ok ? i : i0];
    // }
}