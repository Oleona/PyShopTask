using Grpc.Core;
using GrpcBillingService.Services;


namespace Billing.Services
{

    public class BillingService : Billing.BillingBase
    {
        private static User[] users = new User[]
        {
                new User(new UserProfile(){Name="boris",Amount=0 },5000),
                new User(new UserProfile(){Name="maria",Amount=0 },1000),
                new User(new UserProfile(){Name="oleg",Amount=0 },800),
        };

        public BillingService()
        {
        }

        private static long currentCoinId = 0;

        public override async Task ListUsers(
            None request,
            IServerStreamWriter<UserProfile> responseStream,
            ServerCallContext context
            )
        {
            var profiles = users.Select(l => l.profile).ToArray();
            foreach (var profile in profiles)
            {
                await responseStream.WriteAsync(profile);
            }
        }

        public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
        {
            if (request.Amount < users.Length)
            {
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Emission has not been made. Coins amount is less than users",
                });
            }

            distributeСoins(request.Amount);
            return Task.FromResult(new Response
            {
                Status = Response.Types.Status.Ok,
                Comment = "Emission completed successfully",
            });
        }

        public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
        {
            var srcUser = users.SingleOrDefault(l => l.profile.Name == request.SrcUser);
            var dstUser = users.SingleOrDefault(l => l.profile.Name == request.DstUser);

            if (srcUser == null || dstUser == null)
            {
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Sender or recipient not found",
                });
            }
            if (srcUser.coins.Count < request.Amount)
            {
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Not enough money in the account",
                });
            }

            srcUser.profile.Amount -= request.Amount;
            dstUser.profile.Amount += request.Amount;

            var transferAmount = (int)request.Amount;

            var transferableCoins = new List<Coin>(srcUser.coins.TakeLast(transferAmount)).Select(l => new
            {
                id = l.Id,
                history = l.History + $"{dstUser.profile.Name} "
            });

            srcUser.coins.RemoveRange(srcUser.coins.Count - transferAmount - 1, transferAmount);
            foreach (var transferableCoin in transferableCoins)
            {
                dstUser.coins.Add(new() { Id = transferableCoin.id, History = transferableCoin.history });
            }


            return Task.FromResult(new Response
            {
                Status = Response.Types.Status.Ok,
                Comment = $" Coins from account {srcUser.profile.Name} to account {dstUser.profile.Name} were transferred successfully ",
            });
        }

        public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context) =>
            Task.FromResult(users.SelectMany(u => u.coins).MaxBy(coin => coin.History.Split(" ").Length));



        private void giveOneCoin()
        {
            foreach (var user in users)
            {
                user.profile.Amount++;
                user.coins.Add(new() { Id = currentCoinId++, History = $"{user.profile.Name} " });
            }
        }

        private void distributeRemainingCoins(long remainingCoins, double ratingCoeff)
        {
            users.OrderByDescending(l => l.rating / ratingCoeff - Math.Truncate(l.rating / ratingCoeff));
            for (int i = 0; i < remainingCoins; i++)
            {
                //монет осталось точно меньше, чем пользователей, так что дораспределение более чем по 1 монете не учитываем)
                users[i].profile.Amount++;
                users[i].coins.Add(new() { Id = currentCoinId++, History = $"{users[i].profile.Name} " });
            }
        }

        private void distributeСoins(long totalAmount)
        {
            giveOneCoin();

            long remainingCoins = totalAmount - users.Length;
            long totalRating = users.Select(l => l.rating).Sum();
            double ratingCoeff = (double)totalRating / remainingCoins;

            foreach (var user in users)
            {
                double ratingWeight = user.rating / ratingCoeff;
                if (ratingWeight >= 1)
                {
                    for (int i = 0; i < Math.Floor(ratingWeight); i++)
                    {
                        user.profile.Amount++;
                        user.coins.Add(new() { Id = currentCoinId++, History = $"{user.profile.Name} " });
                        remainingCoins--;
                    }
                }
            }

            distributeRemainingCoins(remainingCoins, ratingCoeff);
        }

    }

}

