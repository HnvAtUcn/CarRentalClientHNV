using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalClientHNV
{
    class Program
    {
        static void Main(string[] args)
        {
            string PortOfTheDay = "25480"; //This port number can be found in the launchSettings.json file in the API solution
            string ServiceURI = "http://localhost:" + PortOfTheDay + "/api/Cars";

            RestClient rest_client = new RestClient();

            /*
           ** RestSharp style
           */

            rest_client.BaseUrl = new Uri(ServiceURI);

            string jsonContents;
            RestRequest request_POST;
            RestRequest request_GET;
            RestRequest request_PUT;
            RestRequest request_DELETE;
            IRestResponse response_POST;
            IRestResponse response_GET;
            IRestResponse response_DELETE;
            try
            {
                // Add 3 new cars to the collection with POST
                var NewCar = new { brand = "Porche", color = "Yellow", active = false };
                request_POST = new RestRequest(ServiceURI, Method.POST);
                // Clever method AddJsonBody that serializes an object into JSON that can be transported in the body of the request
                request_POST.AddJsonBody(NewCar);
                response_POST = rest_client.Execute(request_POST);

                NewCar = new { brand = "Cadillac", color = "Pink", active = false };
                request_POST = new RestRequest(ServiceURI, Method.POST);
                request_POST.AddJsonBody(NewCar);
                response_POST = rest_client.Execute(request_POST);

                NewCar = new { brand = "Trabi", color = "Blue", active = false };
                request_POST = new RestRequest(ServiceURI, Method.POST);
                request_POST.AddJsonBody(NewCar);
                response_POST = rest_client.Execute(request_POST);

                // Get all cars
                request_GET = new RestRequest(ServiceURI, Method.GET);
                response_GET = rest_client.Get(request_GET);
                jsonContents = response_GET.Content;

                JsonDeserializer jd = new JsonDeserializer();
                // Get without {id} will normally return an array - we know for sure in this case
                JsonArray CarsArray = jd.Deserialize<dynamic>(response_GET);

                // Let's find the Trabi and delete it; Nobody ever rents it
                long VarCarID = -1;
                //string VarBrand = "";
                //string VarColor = "";
                //bool VarActive = false;
                foreach (dynamic item in CarsArray)
                {
                    string Brand = item["brand"];
                    if (Brand == "Trabi")
                    {
                        VarCarID = item["carId"];
                        // Not necessary to create a fully populated object for deletion
                        //VarBrand = Brand;
                        //VarColor = item["color"];
                        //VarActive = item["active"];
                    }
                }
                if (VarCarID > -1)
                {
                    var DeleteItem = new { CarID = VarCarID/*, brand = VarBrand, color = VarColor, active = VarActive*/ };
                    request_DELETE = new RestRequest(ServiceURI + "/" + VarCarID.ToString(), Method.DELETE);
                    request_DELETE.AddJsonBody(DeleteItem);
                    response_DELETE = rest_client.Execute(request_DELETE);
                }

                // Get all rented cars (hopefully)
                request_GET = new RestRequest(ServiceURI + "/Rental?rented=true", Method.GET);
                response_GET = rest_client.Get(request_GET);
                jsonContents = response_GET.Content;

                long idFirstRented = -1;
                jd = new JsonDeserializer();
                CarsArray = jd.Deserialize<dynamic>(response_GET);
                foreach (dynamic item in CarsArray)
                {
                    bool active = item["active"];
                    if (active == false)
                    {
                        Console.Out.WriteLine("Something rotten in the state of Denmark!");                
                    }
                    else
                    {
                        if (idFirstRented == -1)
                        {
                            idFirstRented = item["carId"];
                        }
                    }
                }

                // Get all available cars (hopefully)
                request_GET = new RestRequest(ServiceURI + "/Rental?rented=false", Method.GET);
                response_GET = rest_client.Get(request_GET);
                jsonContents = response_GET.Content;

                long idFirstAvailable = -1;
                jd = new JsonDeserializer();
                CarsArray = jd.Deserialize<dynamic>(response_GET);
                foreach (dynamic item in CarsArray)
                {
                    bool active = item["active"];
                    if (active == true)
                    {
                        Console.Out.WriteLine("Something rotten in the state of Denmark!");
                    }
                    else
                    {
                        if (idFirstAvailable == -1)
                        {
                            idFirstAvailable = item["carId"];
                        }
                    }
                }

                // Let's return the first rented car
                request_PUT = new RestRequest(ServiceURI + "/" + idFirstRented.ToString() + "/Returnal", Method.PUT);
                var returnItem = new { carId = idFirstRented  };
                request_PUT.AddJsonBody(returnItem);
                rest_client.Put(request_PUT);

                // Let's rent the first available car
                request_PUT = new RestRequest(ServiceURI + "/" + idFirstAvailable.ToString() + "/Rental", Method.PUT);
                var rentalItem = new { carId = idFirstAvailable };
                request_PUT.AddJsonBody(rentalItem);
                rest_client.Put(request_PUT);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }

            Console.ReadLine();

        }
    }
}

