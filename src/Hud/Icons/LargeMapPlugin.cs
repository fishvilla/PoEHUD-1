using PoeHUD.Controllers;
using PoeHUD.Framework.Helpers;
using PoeHUD.Hud.UI;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using PoeHUD.Poe;
using Map = PoeHUD.Poe.Elements.Map;

namespace PoeHUD.Hud.Icons
{
    public class LargeMapPlugin : Plugin<MapIconsSettings>
    {
        private readonly Func<IEnumerable<MapIcon>> getIcons;

        public LargeMapPlugin(GameController gameController, Graphics graphics, Func<IEnumerable<MapIcon>> gatherMapIcons,
            MapIconsSettings settings)
            : base(gameController, graphics, settings)
        {
            getIcons = gatherMapIcons;
        }

        public override void Render()
        {
            try
            {
                if (!Settings.Enable || !GameController.InGame || !Settings.IconsOnLargeMap
                    || !GameController.Game.IngameState.IngameUi.Map.LargeMap.IsVisible)
                { 
                    return;
                }

                Camera camera = GameController.Game.IngameState.Camera;
                //Console.WriteLine($"Camera- width:{camera.Width} height:{camera.Height} PosX:{camera.Position.X} PosY:{camera.Position.Y} PosZ:{camera.Position.Z} ZFar:{camera.ZFar}");

                Map mapWindow = GameController.Game.IngameState.IngameUi.Map;
                RectangleF mapRect = mapWindow.GetClientRect();

                Vector2 playerPos = GameController.Player.GetComponent<Positioned>().GridPos;
                float posZ = GameController.Player.GetComponent<Render>().Z;

                //Console.WriteLine($"mapRect- Width {mapRect.Width} Height {mapRect.Height} X {mapRect.X} Y {mapRect.Y}");
                //Console.WriteLine($"playerPos- x: {playerPos.X} y: {playerPos.Y} z: {posZ}");


                Vector2 screenCenter = new Vector2(mapRect.Width / 2, mapRect.Height / 2).Translate(0, -20) + new Vector2(mapRect.X, mapRect.Y)
                    + new Vector2(mapWindow.LargeMap.MapShiftX, mapWindow.LargeMap.MapShiftY);
                var diag = (float)Math.Sqrt(camera.Width * camera.Width + camera.Height * camera.Height);
                float k = camera.Width < 1024f ? 1120f : 1024f;
                float scale = k / camera.Height * camera.Width * 3f / 4f / mapWindow.LargeMap.MapZoom;

                //Console.WriteLine($"LargeMap IconsSize: {getIcons().Count()} OnlyVisible Count: {getIcons().Count(x => x.IsVisible())}");

                foreach (MapIcon icon in getIcons().Where(x => x.IsVisible()))
                {
                    //Console.WriteLine($"Going through a map icon");

                    float iconZ = icon.EntityWrapper.GetComponent<Render>().Z;
                    Vector2 point = screenCenter
                        + MapIcon.DeltaInWorldToMinimapDelta(icon.WorldPosition - playerPos, diag, scale, (iconZ - posZ)/(9f/mapWindow.LargeMap.MapZoom));

                    HudTexture texture = icon.TextureIcon;
                    float size = icon.Size * 2;//icon.SizeOfLargeIcon.GetValueOrDefault(icon.Size * 2);
                    texture.Draw(Graphics, new RectangleF(point.X - size / 2f, point.Y - size / 2f, size, size));
                }
            }
            catch (Exception e)
            {
                // do nothing
                PluginLogger.LogError($"Large Map Plugin Exception: {e.ToString()}", 5);
            }
        }
    }
}