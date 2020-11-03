namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            var avaliableItem = itemRepository.FindAll();
            var avaliablePromotion = salesPromotionRepository.FindAll();
            var itemInShoppingCart = new List<(Item item, double itemPrice)>();
            //Dictionary<string, (Item item, int quantity)> shoppingCart = new Dictionary<string, (Item item, int quantity)>();
            var receipt = "============= Order details =============\n";
            double totalPrice = 0;

            foreach (var itemString in inputs)
            {
                (string id, int quantity) itemInfo = Deserialize(itemString);
                var item = avaliableItem.Where(i => i.Id == itemInfo.id).FirstOrDefault();
                var itemPrice = item.Price * itemInfo.quantity;
                receipt = receipt + item.Name + " x " + itemInfo.quantity + " = " + itemPrice + " yuan\n";
                itemInShoppingCart.Add((item, itemPrice));
                totalPrice += itemPrice;
            }

            var validPromotion = GetValidPromotion(itemInShoppingCart, avaliablePromotion);

           
            if (validPromotion.Count != 0)
            {
                receipt = receipt + "-----------------------------------\n";
                var bestDiscount = GetBestDiscount(itemInShoppingCart, avaliablePromotion);
                receipt = receipt +
                    "Promotion used:\n" +
                    $"Half price for certain dishes ({bestDiscount.discountItems}), saving {bestDiscount.bestDiscount} yuan\n";
                totalPrice = totalPrice - bestDiscount.bestDiscount;
            }

            receipt = receipt +
                    "-----------------------------------\n" +
                    $"Total：{totalPrice} yuan\n" +
                    "===================================";

            return receipt;
        }
        //to do: add validation 
        private (string id, int quantity) Deserialize(string input)
        {
            var itemInfo = input.Split(" x ");
            int quantity = int.Parse(itemInfo[1]);
            return (itemInfo[0], quantity);
        }

        private List<SalesPromotion> GetValidPromotion(List<(Item item, double price)> items, List<SalesPromotion> promotions)
        {
            List<SalesPromotion> itemWithPromotion = new List<SalesPromotion>();

            foreach (var promotion in promotions)
            {
                bool existsCheck = promotion.RelatedItems.All(x => items.Any(y => x == y.item.Id));
                if (existsCheck)
                {
                    itemWithPromotion.Add(promotion);
                }
            }
            return itemWithPromotion;
        }

        private (double bestDiscount, string discountItems) GetBestDiscount(List<(Item item, double price)> items, List<SalesPromotion> validPromotions)
        {
            double bestDiscount = 0;
            var bestDiscountItems = "";
            foreach (var promotion in validPromotions)
            {
                double discountPrice = 0;
                var discountItems = "";
                foreach (var id in promotion.RelatedItems)
                {
                    var discountItem = items.Where(i => i.item.Id == id);
                    
                    if (discountItem.Any())
                    {
                        discountItems = discountItems + $" {discountItem.FirstOrDefault().item.Name},";
                        discountPrice += discountItem.FirstOrDefault().price / 2;
                    }
                }
                if (discountPrice > bestDiscount)
                {
                    bestDiscount = discountPrice;
                    bestDiscountItems = discountItems;
                }
            }

            return (bestDiscount, bestDiscountItems.Substring(1, bestDiscountItems.Length - 2));
        }
    }

    public class ItemRepository : IItemRepository
    {
        public List<Item> FindAll()
        {
            throw new NotImplementedException();
        }
    }

    public class SalesPromotionRepository : ISalesPromotionRepository
    {
        List<SalesPromotion> ISalesPromotionRepository.FindAll()
        {
            throw new NotImplementedException();
        }
    }
}
