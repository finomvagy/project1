using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookCatalog
{
    public class Book
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; } = new List<Category>();

        [JsonIgnore]
        public List<int> CategoryIds
        {
            get { return Categories?.Select(c => c.Id).ToList() ?? new List<int>(); }
        }

        public Book() { }
        public Book(string author, string title, string details)
        {
            Author = author;
            Title = title;
            Details = details;
        }

        public string CategoriesDisplayText
        {
            get
            {
                if (Categories == null || !Categories.Any())
                {
                    return "No categories";
                }
                return string.Join(", ", Categories.Select(c => c.Name));
            }
        }
    }
}