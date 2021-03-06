using System;
using System.Collections.Generic;
using System.Linq;
using BotTemplate.DAL.Entities;
using TradingBot.Domain.Shared.Dto;
using TradingBot.Exchange.Domain.Models.Response;

namespace BotTemplate.Domain.Dto {
    public interface ITradingPairContext : ITradingPairBaseContext<ConcreteTradingPair> {
    }
    
    public class TradingPairContext : ITradingPairContext {
        private CandleStickResponse _latestCandleStick;
        
        public Guid RunId { get; set; }
        public ConcreteTradingPair TradingPair { get; set; }
        public IEnumerable<CandleStickResponse> LatestCandleSticks { get; set; }
        public IEnumerable<CandleStickResponse> AllCandleSticks { get; set; }
        public CandleStickResponse CurrentCandleStick => this._latestCandleStick ??= this.LatestCandleSticks.LastOrDefault();

        public bool HasCandleSticks { get; set; }
    }
}