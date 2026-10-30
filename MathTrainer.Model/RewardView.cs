using MathTrainer.Model;

namespace MathTrainer.Model
{
    public class RewardView
    {
        public Reward Reward { get; }
        public bool IsAffordable { get; set; }

        public Guid Id => Reward.Id;
        public string Name => Reward.Name;
        public string ImagePath => Reward.ImagePath;
        public int Price => Reward.Price;

        public RewardView(Reward reward, int childMedals)
        {
            Reward = reward;
            IsAffordable = childMedals >= Reward.Price;
        }
    }
}