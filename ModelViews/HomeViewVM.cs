using DemoMongoDB.Models;

namespace DemoMongoDB.ModelViews
{
    public class HomeViewVM
    {
        public List<News> News { get; set; }
        public List<Banners> Banners { get; set; }
        public List<Categories> Categories { get; set; }

    }
}
