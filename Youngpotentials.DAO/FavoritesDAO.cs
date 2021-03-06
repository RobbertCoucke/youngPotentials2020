﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Youngpotentials.Domain.Entities;

namespace Youngpotentials.DAO
{
    public interface IFavoritesDAO
    {
        IEnumerable<Favorites> GetAllFavoritesFromUserId(int id);
        Favorites AddFavorite(int userId, int offerId);
        void DeleteFavorite(int id);
        IEnumerable<Favorites> GetAllFavoritesFromOfferId(int id);
    }
    public class FavoritesDAO : IFavoritesDAO
    {

        private YoungpotentialsV1Context _db;

        public FavoritesDAO()
        {
            _db = new YoungpotentialsV1Context();
        }
        public Favorites AddFavorite(int userId, int offerId)
        {
            try
            {
                var favorite = new Favorites();
                favorite.StudentId = userId;
                favorite.OfferId = offerId;
                _db.Entry(favorite).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                _db.SaveChanges();
                return favorite;
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public void DeleteFavorite(int id)
        {
            try
            {
                var favorite =_db.Favorites.Where(f => f.Id == id).FirstOrDefault();
                if(favorite != null)
                {
                    _db.Favorites.Remove(favorite);
                }
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }


        //gets all favorites from offerId
        public IEnumerable<Favorites> GetAllFavoritesFromOfferId(int id)
        {
            try
            {
                return _db.Favorites.Where(f => f.OfferId == id).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }


        //gets all favorites from userId
        public IEnumerable<Favorites> GetAllFavoritesFromUserId(int id)
        {
            try
            {
                return _db.Favorites.Where(f => f.StudentId == id).Include(f => f.Offer).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
}
