using System;

namespace SmartLab.BGS2.Core
{
    public class BufferedArray
    {
        private const int EXPANDSIZE = 1024;

        /// <summary>
        /// payload length not include the checksum
        /// </summary>
        protected int position = 0;

        /// <summary>
        /// Frame specifi data.
        /// payload content not include the checksum, the valid length is indicated by this.Length
        /// !! do not use FrameData.Length, this is not the packet's payload length
        /// </summary>
        protected byte[] data;

        /// <summary>
        /// Copy construct
        /// </summary>
        /// <param name="buffer">the source buffer array</param>
        public BufferedArray(BufferedArray buffer) 
        {
            if (buffer != null)
            {
                this.data = buffer.data;
                this.position = buffer.position;
            }
        }

        public BufferedArray(int size) { this.data = new byte[size]; }

        public BufferedArray() { this.data = new byte[EXPANDSIZE]; }
        
        public virtual int GetPosition() { return this.position; }

        public virtual void SetPosition(int position) 
        {
            if (position >= this.data.Length)
                this.position = this.data.Length;
            else this.position = position; 
        }

        public virtual void Allocate(int length)
        {
            if (length <= 0)
                return;

            if (length > this.data.Length)
                this.data = new byte[length];

            this.Rewind();
        }

        public virtual void Rewind() { this.position = 0; }

        /// <summary>
        /// write the value into the current posiont and the posiont + 1
        /// will create more space if position overflow
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetContent(byte value)
        {
            SetContent(this.position, value);
            this.position++;
        }

        /// <summary>
        /// write the value into anywhere and the current positon not affected
        /// will create more space if position overflow
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public virtual void SetContent(int index, byte value)
        {
            if (index < 0)
                return;

            if (index >= this.data.Length)
                ExpandSpace(1);

            this.data[index] = value;
        }

        /// <summary>
        /// write the value into the current posiont and the posiont + value.length
        /// will create more space if position overflow
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetContent(byte[] value) { SetContent(value, 0, value.Length); }

        /// <summary>
        /// write the value into anywhere and the current positon not affected
        /// will create more space if position overflow
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public virtual void SetContent(int index, byte[] value) { SetContent(index, value, 0, value.Length); }

        /// <summary>
        /// write the value into the current posiont and the posiont + length
        /// will create more space if position overflow
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public virtual void SetContent(byte[] value, int offset, int length)
        {
            SetContent(position, value, offset, length);
            position += length;
        }

        /// <summary>
        /// write the value into anywhere and the current positon not affected
        /// will create more space if position overflow
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public virtual void SetContent(int index, byte[] value, int offset, int length)
        {
            if (index + length - offset > this.data.Length)
                ExpandSpace(index + length - offset - this.data.Length);

            Array.Copy(value, 0, this.data, index, length);
        }

        public byte[] GetFrameData()
        {
            return this.data;
        }
        
        private void ExpandSpace(int length)
        {
            byte[] temp = this.data;
            this.data = new byte[this.data.Length + EXPANDSIZE * (1 + length / EXPANDSIZE)];
            Array.Copy(temp, this.data, this.position);
        }
    }
}