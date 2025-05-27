using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BookCatalog
{
    public class Category
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class CategoryStat
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("bookCount")]
        public int BookCount { get; set; }
    }
}