using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Net;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Dots.Controllers
{
    
    public class Point
    {
        public Point(int x, int y)
        { 
            this.x = x;
            this.y = y;
        }
        public Point()
        {
            this.x = 0;
            this.y = 0;
        }
        public int x { get; set; }
        public int y { get; set; }


        public override string ToString()
        {
            return $"{this.x}\t{this.y}";
        }

        public bool line(Point p1, Point p2)
        {
            float x = this.x;
            float y = this.y;
            float x1 = p1.x;
            float y1 = p1.y;
            float x2 = p2.x;
            float y2 = p2.y;
            float res = (x - x1) / (x2 - x1) - (y - y1) / (y2 - y1);
            if (res < 0.1 && res > -0.1)
            {
                return true;
            }
            return false;
        }

        public void rotateClock(Point center)
        {
            int oldX = this.x;
            int oldY = this.y;
            this.x = center.x + center.y - oldY;
            this.y = center.x - center.y + oldX;
        }
        public void rotateCounterClock(Point center)
        {
            int oldX = this.x;
            int oldY = this.y;
            this.x = center.x - center.y + oldY;
            this.y = center.x + center.y - oldX;
        }
        public void reverse(Point center)
        {
            if (this.x > center.x)
            {
                this.x = center.x - (this.x - center.x);
            }
            else
            {
                this.x = center.x + (center.x - this.x);
            }
        }
        public static ICollection<Point> adjustCollection(ICollection<Point> points)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (Point point in points)
            {
                if (point.x < minX)
                {
                    minX = point.x;
                }
                if (point.y < minY)
                {
                    minY = point.y;
                }
            }
            foreach (Point point in points)
            {
                point.x += -minX;
                point.y += -minY;
            }
            return points;
        }
        public static Point findCenterInCollection(ICollection<Point> points)
        {
            int maxX = int.MinValue, maxY = int.MinValue, minX = int.MaxValue, minY = int.MaxValue;

            foreach (Point point in points)
            {
                if (point.x > maxX)
                {
                    maxX = point.x;
                }
                if (point.x < minX)
                {
                    minX = point.x;
                }
                if (point.y < minY)
                {
                    maxY = point.y;
                }
                if (point.y < minY)
                {
                    minY = point.y;
                }
            }

            return new Point( (maxX - minX) / 2, (maxY - minY) / 2);
        }
        public static ICollection<Point> stringToCollection(string text)
        {
            ICollection<Point> result = new List<Point>();
            MatchCollection matched = new Regex(@"\d+\t\d+").Matches(text);
            foreach (Match m in matched)
            {
                MatchCollection matched2 = new Regex(@"\d+").Matches(m.Value);
                result.Add(new Point(int.Parse(matched2[0].Value), int.Parse(matched2[1].Value)));
            }

            return result;
        }
        public static string collectionToString(ICollection<Point> points)
        {
            string result = "X\tY";
            foreach (Point p in points)
            {
                result += "\n" + p.ToString();
            }
            return result;
        }
    }

    [Route("api")]
    [ApiController]
    public class DotsController : ControllerBase
    {
        [HttpPost("line")]
        public async Task<FileResult> Line(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                ICollection<Point> points = Point.stringToCollection(await reader.ReadToEndAsync());
                Point center = Point.findCenterInCollection(points);
                Random _rand = new Random();
                Point p1 = new Point(_rand.Next(1, 100), _rand.Next(1, 100));
                Point p2 = new Point(_rand.Next(1, 100), _rand.Next(1, 100));
                //Point p1 = new Point(0, 0);
                //Point p2 = new Point(100, 100);
                ICollection<Point> newPoints = new List<Point>();
                foreach (Point p in points)
                {
                    if (p.line(p1,p2))
                    {
                        newPoints.Add(p);
                    }
                }
                newPoints.Add(p1);
                newPoints.Add(p2);
                string res = Point.collectionToString(newPoints);

                var contentType = "text/txt";
                var bytes = Encoding.UTF8.GetBytes(res);
                var result = new FileContentResult(bytes, contentType);
                result.FileDownloadName = "File.txt";
                return result;
            }
        }
        [HttpPost("flipClock")]
        public async Task<FileResult> FlipClock(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                ICollection<Point> points = Point.stringToCollection(await reader.ReadToEndAsync());
                Point center = Point.findCenterInCollection(points);
                foreach (Point p in points)
                {
                    p.rotateClock(center);
                }
                points = Point.adjustCollection(points);
                string res = Point.collectionToString(points);

                var contentType = "text/txt";
                var bytes = Encoding.UTF8.GetBytes(res);
                var result = new FileContentResult(bytes, contentType);
                result.FileDownloadName = "File.txt";
                return result;
            }
        }
        [HttpPost("flipCounterClock")]
        public async Task<FileResult> FlipCounterClock(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                ICollection<Point> points = Point.stringToCollection(await reader.ReadToEndAsync());
                Point center = Point.findCenterInCollection(points);
                foreach (Point p in points)
                {
                    p.rotateCounterClock(center);
                }
                points = Point.adjustCollection(points);
                string res = Point.collectionToString(points);

                var contentType = "text/txt";
                var bytes = Encoding.UTF8.GetBytes(res);
                var result = new FileContentResult(bytes, contentType);
                result.FileDownloadName = "File.txt";
                return result;
            }
        }

        [HttpPost("reverse")]
        public async Task<FileResult> Reverse(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                ICollection<Point> points = Point.stringToCollection(await reader.ReadToEndAsync());
                Point center = Point.findCenterInCollection(points);
                foreach (Point p in points)
                {
                    p.reverse(center);
                }
                points = Point.adjustCollection(points);
                string res = Point.collectionToString(points);

                var contentType = "text/txt";
                var bytes = Encoding.UTF8.GetBytes(res);
                var result = new FileContentResult(bytes, contentType);
                result.FileDownloadName = "File.txt";
                return result;
            }
        }
        [HttpPost("adjust")]
        public async Task<FileResult> adjust(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                ICollection<Point> points = Point.stringToCollection(await reader.ReadToEndAsync());
                points = Point.adjustCollection(points);
                string res = Point.collectionToString(points);

                var contentType = "text/txt";
                var bytes = Encoding.UTF8.GetBytes(res);
                var result = new FileContentResult(bytes, contentType);
                result.FileDownloadName = "File.txt";
                return result;
            }
        }
    }

}
