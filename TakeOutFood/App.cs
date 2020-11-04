namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection.Metadata.Ecma335;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        //to do: imporve performance 
        public string BestCharge(List<string> inputs)
        {
            var avaliableItems = itemRepository.FindAll();
            var avaliablePromotions = salesPromotionRepository.FindAll();
            var itemInShoppingCart = new List<(Item item, double itemPrice)>();
            var receipt = "============= Order details =============\n";
            double totalPrice = 0;

            foreach (var itemString in inputs)
            {
                var itemInfo = DeserializeInputStringToIdAndQuantity(itemString);
                if (itemInfo == null) continue;

                var item = avaliableItems.Where(i => i.Id == itemInfo.Value.id).FirstOrDefault();
                var itemPrice = item.Price * itemInfo.Value.quantity;

                itemInShoppingCart.Add((item, itemPrice));
                totalPrice += itemPrice;
                receipt = receipt + $"{item.Name} x {itemInfo.Value.quantity} = {itemPrice} yuan\n";
            }

            var validPromotion = GetValidPromotion(itemInShoppingCart, avaliablePromotions);

            if (validPromotion.Count != 0)
            {
                receipt = receipt + "-----------------------------------\n";
                var bestDiscount = GetBestDiscount(itemInShoppingCart, avaliablePromotions);
                receipt = receipt +
                      $"Promotion used:\n{bestDiscount.promotionName} ({bestDiscount.discountItems}), saving {bestDiscount.totalDiscount} yuan\n";
                totalPrice = totalPrice - bestDiscount.totalDiscount;
            }

            receipt = receipt +
                    "-----------------------------------\n" +
                    $"Total：{totalPrice} yuan\n" +
                    "===================================";

            return receipt;
        }

        private (string id, int quantity)? DeserializeInputStringToIdAndQuantity(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return null;
            }
            try
            {
                var itemInfo = input.Split(" x ");
                var id = itemInfo[0];
                int quantity = int.Parse(itemInfo[1]);
                return (id, quantity);
            }
            catch
            {
                return null;
            }

        }

        //Not sure if I understand it correctly. 
        //Originally I think the valid promotion is when all promotion items are included in input items. 
        //This version is: the valid promotion is when the at least one promotions are include in input items. 
        private List<SalesPromotion> GetValidPromotion(List<(Item item, double price)> items, List<SalesPromotion> promotions)
        {
            List<SalesPromotion> validPromotion = new List<SalesPromotion>();

            foreach (var promotion in promotions)
            {
                bool existsCheck = promotion.RelatedItems.Any(p => items.Any(i => p == i.item.Id));
                if (existsCheck)
                {
                    validPromotion.Add(promotion);
                }
            }
            return validPromotion;
        }

        private (double totalDiscount, string discountItems, string promotionName) GetBestDiscount(List<(Item item, double price)> items, List<SalesPromotion> validPromotions)
        {
            double bestDiscount = 0;
            var bestDiscountItems = new List<Item>();
            var bestPromotion = "";
            
            foreach (var promotion in validPromotions)
            {
                double discountPrice = 0;
                var discountItems = new List<Item>();
                Func<double, double> discountFunc;

                switch(promotion.Type){
                    case "50%_DISCOUNT_ON_SPECIFIED_ITEMS":
                        discountFunc = (totalprice) => totalprice / 2;
                        break;
                    default:
                        continue;
                }

                foreach (var id in promotion.RelatedItems)
                {
                    var item = items.Where(i => i.item.Id == id);
                    if (item.Any())
                    {
                        discountItems.Add(item.FirstOrDefault().item);
                        discountPrice += discountFunc(item.FirstOrDefault().price);
                    }
                }
                if (discountPrice > bestDiscount)
                {
                    bestDiscount = discountPrice;
                    bestDiscountItems = discountItems;
                    bestPromotion = promotion.DisplayName;
                }
            }
            return (bestDiscount, ItemsToString(bestDiscountItems), bestPromotion);
        }

        private string ItemsToString(List<Item> items)
        {
            string itemsString = "";
            if (items.Count < 1)
            {
                return itemsString;
            }
            foreach (var item in items)
            {
                itemsString += $"{item.Name}, ";
            }
            return itemsString[0..^2];
        }

        ////to do: imporve performance 
        //public string BestCharge(List<string> inputs)
        //{
        //    var avaliableItems = itemRepository.FindAll();
        //    var avaliablePromotions = salesPromotionRepository.FindAll();
        //    //var itemInShoppingCart = new List<(Item item, double itemPrice)>();
        //    var receipt = "============= Order details =============\n";
        //    double totalPrice = 0;

        //    Dictionary<string, (Item item, double itemPrice)> itemInShoppingCart = new Dictionary<string, (Item item, double itemPrice)>();
        //    foreach (var itemString in inputs)
        //    { 
        //        var itemInfo = DeserializeInputStringToIdAndQuantity(itemString);
        //        if (itemInfo == null) continue;

        //        var item = avaliableItems.Where(i => i.Id == itemInfo.Value.id).FirstOrDefault();
        //        var itemPrice = item.Price * itemInfo.Value.quantity;


        //        if (!itemInShoppingCart.ContainsKey(item.Id))
        //        {
        //            itemInShoppingCart.Add(item.Id, (item, itemPrice));
        //        }
        //        else
        //        {
        //            itemInShoppingCart[item.Id] = (item, itemPrice + itemInShoppingCart[item.Id].itemPrice);
        //        }
        //        totalPrice += itemPrice;
        //        receipt = receipt + $"{item.Name} x {itemInfo.Value.quantity} = {itemPrice} yuan\n";
        //    }

        //    var validPromotion = GetValidPromotion(itemInShoppingCart, avaliablePromotions);

        //    if (validPromotion.Count != 0)
        //    {
        //        receipt = receipt + "-----------------------------------\n";
        //        var bestDiscount = GetBestDiscount(itemInShoppingCart, avaliablePromotions);
        //        receipt = receipt +
        //              $"Promotion used:\n {bestDiscount.} ({bestDiscount.discountItems}), saving {bestDiscount.bestDiscount} yuan\n";
        //        totalPrice = totalPrice - bestDiscount.bestDiscount;
        //    }

        //    receipt = receipt +
        //            "-----------------------------------\n" +
        //            $"Total：{totalPrice} yuan\n" +
        //            "===================================";

        //    return receipt;
        //}

    }
}
