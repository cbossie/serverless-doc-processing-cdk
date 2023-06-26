﻿using Amazon.S3.Model;
using System.Linq;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared.AwsSdkUtilities
{
    public static class AwsSdkExtensions
    {
        // Tagging Extensions
        public static string? GetTagValue(this List<Tag> tagging, string key) =>
            tagging.FirstOrDefault(a => a.Key == key)?.Value;

        public static IEnumerable<string> GetTagValueList(this List<Tag> tagging, string key, string separator = ",") =>
            tagging.FirstOrDefault(a => a.Key == key)?.Value?.Split(separator) ?? Enumerable.Empty<string>();

    }
}
