namespace AppUpdatedXamarin.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IdImageItem { get; set; }
        public string Price { get; set; }

        public Item(string id, string name, string description, string image, string price)
        {
            Id = id;
            Name = name;
            Description = description;
            IdImageItem = image;
            Price = price;
        }

        public Item()
        {
        }
    }
}