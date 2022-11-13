using Billing;

namespace GrpcBillingService.Services
{
    public class User
    {
        public readonly UserProfile profile;
        public readonly int rating;
        public readonly List<Coin> coins;

        public User(UserProfile profile, int rating)
        {
            this.profile = profile;
            this.rating = rating;
            this.coins = new List<Coin>();
        }
    }
}
