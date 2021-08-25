using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Drawing;
using System.Collections;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace CharacomEx
{
	#region インターフェイス(ICommand)
	public interface ICommand
	{
		void Action();
		void Undo();
		void Redo();
	}
	#endregion

	/// <summary>
	/// Undo Redoのためのコマンドマネジャー
	/// </summary>
	class CommandManager
    {
		private Stack<ICommand> undo = new Stack<ICommand>();
		private Stack<ICommand> redo = new Stack<ICommand>();

		public void Action(ICommand command)
		{
			command.Action();
			this.undo.Push(command);
			System.Diagnostics.Debug.WriteLine($"Push undo.count = {undo.Count}");
			this.redo.Clear();
		}

		public void UndoClear()
		{
			this.undo.Clear();
		}

		public void RedoClear()
		{
			this.redo.Clear();
		}

		public void Undo()
		{
			if (!this.CanUndo()) return;
			ICommand command = this.undo.Pop();
			command.Undo();
			this.redo.Push(command);
		}

		public void Redo()
		{
			if (!this.CanRedo()) return;
			ICommand command = this.redo.Pop();
			command.Redo();
			this.undo.Push(command);
		}
		/// <summary>
		/// 元に戻せるかどうかを確認する
		/// </summary>
		/// <returns>元に戻せるかどうか</returns>
		public bool CanUndo()
		{
			return 0 < this.undo.Count;
		}

		/// <summary>
		/// やり直せるかどうかを確認する
		/// </summary>
		/// <returns>やり直せるかどうか</returns>
		public bool CanRedo()
		{
			return 0 < this.redo.Count;
		}
	}

	#region 【CharaTabItem】ペン、消しゴムによる描画
	public class DrawingCanvasCommand : ICommand
	{
		private RenderTargetBitmap _setBmp;
		private Image _nextImg;
		private Image _beforeImg;
		private BitmapSource _nextBmp;
		private BitmapSource _beforeBmp;
		private BitmapSource _nextProc;
		private BitmapSource _beforeProc;
		private InkCanvas _outSrcCanvas;
		private Canvas _outPrcCanvas;
		ImageEffect imageEffect = new ImageEffect();

		public DrawingCanvasCommand(Image ProjectImg, BitmapSource srcBmp, RenderTargetBitmap setBmp, InkCanvas outSouceCanvas, Canvas outProcCanvas)
        {
			_outSrcCanvas = outSouceCanvas;
			_outPrcCanvas = outProcCanvas;
			_setBmp = setBmp;
			_nextImg = ProjectImg;
			_nextBmp = srcBmp;
			_nextProc = srcBmp;
			_beforeImg = ProjectImg;
			_beforeBmp = srcBmp;
			
		}


		public void Action()
		{
			//Beforeを作る
			//_beforeImg = _nextImg;
			//_beforeBmp = _nextBmp;
			
			//描画を反映する
			_nextImg.Source = _setBmp;
			_nextBmp = _setBmp;

			ViewImages();
			System.Diagnostics.Debug.WriteLine($"Action ! - DrawingCanvasCommand {_nextBmp.Width},{_nextBmp.Height}");
		}

		public void Undo()
		{
			//戻す
			_nextImg = _beforeImg;
			_nextBmp = _beforeBmp;

			ViewImages();
			System.Diagnostics.Debug.WriteLine("Undo ! - DrawingCanvasCommand");
		}

		public void Redo()
		{
			//再反映
			_nextImg.Source = _setBmp;
			_nextBmp = _setBmp;

			ViewImages();
			System.Diagnostics.Debug.WriteLine("Redo ! - DrawingCanvasCommand");
		}

		public void ViewImages()
        {
			//画像処理用のビットマップを作成
			BitmapSource proc_bmp;
			var Oya = (MainWindow)Application.Current.MainWindow;

			imageEffect.GetGravityPointDouble((BitmapSource)_nextBmp);
			proc_bmp = (BitmapSource)_nextBmp;
			if (Oya.MenuNomalizeCheck.IsChecked == true) proc_bmp = imageEffect.Normalize(proc_bmp, 96 + 38 + 96);
			if (Oya.MenuCenterCheck.IsChecked == true) proc_bmp = imageEffect.MoveCenter_fromGravity(proc_bmp);

			//2021.08.08 D.Honjyou
			//CachedBitmapになってしまったのを回避するためいったんバイナリに変換
			byte[] tmp = imageEffect.ToBinary(_nextBmp);
			BitmapSource srcBmp = imageEffect.ToBitmapSource(tmp);

			ImageBrush imageBrush = new ImageBrush();
			imageBrush.Stretch = Stretch.None;
			imageBrush.ImageSource = (BitmapImage)srcBmp;

			_outSrcCanvas.Background = imageBrush;

			//2021.08.08 D.Honjyou
			//CachedBitmapになってしまったのを回避するためいったんバイナリに変換
			tmp = imageEffect.ToBinary(proc_bmp);
			srcBmp = imageEffect.ToBitmapSource(tmp);

			imageBrush = new ImageBrush();
			imageBrush.Stretch = Stretch.None;
			imageBrush.ImageSource = (BitmapImage)srcBmp;

			_outPrcCanvas.Background = imageBrush;
		}
	}
	#endregion
}
