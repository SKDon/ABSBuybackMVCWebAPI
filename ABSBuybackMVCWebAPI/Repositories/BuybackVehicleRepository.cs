﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ABSBuybackMVCWebAPI.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using ABSBuybackMVCWebAPI.Utilities;

namespace ABSBuybackMVCWebAPI.Repositories
{
    class BuybackVehicleRepository : IBuybackVehicleRepository
    {
        private const string Select = @"SELECT gs.SaleLocation, g2.SaleFirstDate ,g.VehicleID, dbo.WhoAMI(g.DealerID) AS Seller, g.DealerID AS SellerId, g2.SaleID AS SaleLocationId, dbo.WhoAMI(g1.DealerID) Buyer, g1.DealerID AS BuyerId, g.BidSheetNumber, dbo.YMM(g.VehicleID) YMM, RIGHT(g.VIN,6) VIN "
                                        + FromAndJoins;

        private const string FromAndJoins = @"FROM GSV g
                                            JOIN GSI g2 ON g.SaleInstanceID = g2.SaleInstanceID
                                            JOIN GSB g1 ON g.VehicleID = g1.VehicleID AND g1.WinningBid = 1
                                            JOIN ABSContact.dbo.CONTACT2 c ON g1.DealerID = c.ACCOUNTNO-- AND c.BuybackBidder = 1
                                            JOIN GroupSale gs ON g2.SaleID = gs.SaleID AND gs.ForDropDown = 1
                                            JOIN GroupSaleParticipants gsp 
                                                  ON g.SaleInstanceID = gsp.SaleInstanceID 
                                                  AND g.DealerID = gsp.DealerID 
                                                  AND (gsp.BidsCompleted = 1 OR gsp.DealerPrelimsDone = 1)
                                            JOIN GroupSaleVehiclesAdditionalData AD ON g.VehicleID = AD.VehicleID AND AD.AbsInventory = 1
                                            LEFT JOIN Buyback b ON g.VehicleID = b.VehicleIdOriginal";
        private const string SelectPaged = @"SELECT SaleLocation, SaleFirstDate, VehicleID, Seller, SellerId, SaleLocationId, Buyer, BuyerId, BidSheetNumber, YMM, VIN 
                                            FROM(
	                                            SELECT gs.SaleLocation, g2.SaleFirstDate ,g.VehicleID, dbo.WhoAMI(g.DealerID) AS Seller, g.DealerID AS SellerId, g2.SaleID AS SaleLocationId, dbo.WhoAMI(g1.DealerID) Buyer, g1.DealerID AS BuyerId, g.BidSheetNumber, dbo.YMM(g.VehicleID) YMM, RIGHT(g.VIN,6) VIN, ROW_NUMBER()
		                                            OVER (ORDER BY gs.SaleLocation, SaleFirstDate DESC, dbo.WhoAMI(g.DealerID), g.BidSheetNumber) AS RowNum "
                                                    + FromAndJoins
		                                            + @"{0}
	                                            ) AS BBV
                                            WHERE BBV.RowNum BETWEEN (({1}-1)*{2})+1
                                            AND {2}*({1})";
        private const string WhereTemplate = @" WHERE b.VehicleIdOriginal IS NULL AND g2.salefirstdate BETWEEN DATEADD(DAY, -60,GETDATE()) AND GETDATE(){0}";
        private string WherePredicate = "";
        private const string OrderBy = @" ORDER BY gs.SaleLocation,SaleFirstDate DESC, Seller, g.BidSheetNumber";
        private readonly IBuybackVehicleQueryProcessorFactory buybackVehicleQueryProcessorFactory;

        public BuybackVehicleRepository(IBuybackVehicleQueryProcessorFactory buybackVehicleQueryProcessorFactory)
        {
            this.buybackVehicleQueryProcessorFactory = buybackVehicleQueryProcessorFactory;
        }

        public List<BuybackVehicle> GetAll()
        {
            var query = FormQuery();
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABS-SQL"].ConnectionString))
            {
                return connection.Query<BuybackVehicle>(query).ToList();
            }
        }

        private string FormQuery()
        {
            return Select + String.Format(WhereTemplate, WherePredicate) + OrderBy;
        }

        private string FormQuery(int pageNumber, int pageSize)
        {
            var whereClause = String.Format(WhereTemplate, WherePredicate);
            return String.Format(SelectPaged, whereClause, pageNumber, pageSize);
        }

        public IEnumerable<BuybackVehicle> Paged(int pageSize, int pageNumber)
        {
            var query = FormQuery(pageNumber, pageSize);
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABS-SQL"].ConnectionString))
            {
                return connection.Query<BuybackVehicle>(query).ToList();
            }
        }

        public BuybackVehicle Get(int id)
        {
            throw new NotImplementedException();
        }

        public int Insert(BuybackVehicle poco)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABS-SQL"].ConnectionString))
            {
                return (int)connection.Insert(poco);
            }
        }

        public int Insert(BuybackVehicle poco, IDbTransaction transaction)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABS-SQL"].ConnectionString))
            {
                return (int)connection.Insert(poco, transaction);
            }
        }

        public bool Update(BuybackVehicle poco)
        {
            throw new NotImplementedException();
        }

        public int Delete(BuybackVehicle poco)
        {
            throw new NotImplementedException();
        }

        public List<BuybackVehicle> Search(BuybackVehicleQuery queryObject)
        {
            WherePredicate = buybackVehicleQueryProcessorFactory.Create(queryObject).ProcessAll();
            var query = FormQuery();
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABS-SQL"].ConnectionString))
            {
                return connection.Query<BuybackVehicle>(query).ToList();
            }
        }


        public IEnumerable<BuybackVehicle> SearchPaged(BuybackVehicleQuery queryObject, int pageSize, int pageNumber)
        {
            WherePredicate = buybackVehicleQueryProcessorFactory.Create(queryObject).ProcessAll();
            var query = FormQuery(pageNumber, pageSize);
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABS-SQL"].ConnectionString))
            {
                return connection.Query<BuybackVehicle>(query).ToList();
            }
        }
    }
}
