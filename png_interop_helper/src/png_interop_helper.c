
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>

#include <png.h>

static void user_warning_fn(png_structp png_ptr, png_const_charp warning_msg)
{} /* ignore warnings */

static void user_error_fn(png_structp png_ptr, png_const_charp error_msg)
{
    const char** error_message = png_get_error_ptr(png_ptr);
    *error_message = error_msg;
    longjmp(png_jmpbuf(png_ptr), 1);
}

int32_t write_png_to_file(char* filename, uint32_t width, uint32_t height, uint32_t bit_width, uint32_t color_type, uint32_t interlace_type, uint32_t compression_type, uint32_t filter_type, uint32_t transform_type, uint8_t* image_data, uint32_t row_size, const char** perror_message)
{
    FILE *fp = fopen(filename, "wb");
    if (fp == NULL)
    {
        *perror_message = strerror(errno);
        return -1;
    }    

    png_structp png_ptr = png_create_write_struct(PNG_LIBPNG_VER_STRING, perror_message, user_warning_fn, user_error_fn);
    if (png_ptr == NULL)
    {
        fclose(fp);
        return -2;
    }

    png_infop info_ptr = png_create_info_struct(png_ptr);
    if (info_ptr == NULL)
    {
        fclose(fp);
        png_destroy_write_struct(&png_ptr,  NULL);
        return -3;
    }

    if (setjmp(png_jmpbuf(png_ptr)))
    {
        /* If we get here, we had a problem writing the file */
        png_destroy_write_struct(&png_ptr, &info_ptr);
        fclose(fp);
        return -4;
    }

    png_init_io(png_ptr, fp);

    uint8_t** rows = malloc(sizeof(uint8_t*) * height);
    if (rows == NULL)
    {
        png_destroy_write_struct(&png_ptr, &info_ptr);
        fclose(fp);
        return -5;
    }

    for (int i = 0; i < height; i++)
        rows[i] = image_data + row_size * i;

    png_set_IHDR(png_ptr, info_ptr, width, height, bit_width, color_type, interlace_type, compression_type, filter_type);
    png_set_rows(png_ptr, info_ptr, rows);
    png_write_png(png_ptr, info_ptr, transform_type, NULL);

    png_destroy_write_struct(&png_ptr, &info_ptr);
    free(rows);
    fclose(fp);

    return 0;
}

typedef void (*write_cb)(uint8_t* data, size_t size);

int32_t write_png_with_cb(write_cb cb, uint32_t width, uint32_t height, uint32_t bit_width, uint32_t color_type, uint32_t interlace_type, uint32_t compression_type, uint32_t filter_type, uint32_t transform_type, uint8_t* image_data, uint32_t row_size, const char** perror_messafe)
{
    return -1000;
}

