using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BijliSaver.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUrduExplanations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Simple everyday Urdu, mirroring the English explanations —
            // written for the same audience as "bill samjho, paisa bachao".
            migrationBuilder.Sql("""
                UPDATE charge_definitions AS c SET explanation_ur = v.ur
                FROM (VALUES
                    ('ENERGY',        'آپ نے جتنی بجلی استعمال کی اس کی اصل قیمت، آپ کے ٹیرف سلیب کے حساب سے۔'),
                    ('FPA',           'جب بجلی بنانے والا ایندھن مہنگا ہو جائے تو لگنے والا اضافی چارج — سستا ہونے پر رعایت بھی ملتی ہے۔'),
                    ('FC_SURCHARGE',  'حکومت کی طرف سے ایندھن کی قیمت سے متعلق ایک اضافی سرچارج۔'),
                    ('QTR_ADJ',       'ریگولیٹر کی منظوری سے ہر تین ماہ بعد لاگو ہونے والی شرح کی ایڈجسٹمنٹ۔'),
                    ('METER_RENT',    'آپ کے گھر لگے بجلی کے میٹر کا مقررہ ماہانہ کرایہ۔'),
                    ('SERVICE_RENT',  'آپ کے گھر تک بجلی کے کنکشن کی دیکھ بھال کی مقررہ ماہانہ فیس۔'),
                    ('ED',            'صوبائی حکومت کا بجلی کے استعمال پر ٹیکس۔'),
                    ('TVFEE',         'ٹی وی رکھنے کی سرکاری فیس جو بجلی کے بل کے ساتھ لی جاتی ہے۔'),
                    ('GST',           'وفاقی سیلز ٹیکس — بالکل ویسے جیسے خریداری پر ٹیکس لگتا ہے۔'),
                    ('INCOME_TAX',    'انکم ٹیکس کی مد میں پیشگی کٹوتی جو بل کے ذریعے لی جاتی ہے۔'),
                    ('EXTRA_TAX',     'نان فائلر صارفین پر لگنے والا اضافی وفاقی ٹیکس۔'),
                    ('FURTHER_TAX',   'غیر رجسٹرڈ کاروباروں پر لگنے والا اضافی سیلز ٹیکس۔'),
                    ('RS_TAX',        'ریٹیل ٹیرف صارفین سے ٹیکس اتھارٹی کے لیے وصول کیا جانے والا ٹیکس۔'),
                    ('GST_ON_FPA',    'فیول پرائس ایڈجسٹمنٹ کی رقم پر لگنے والا سیلز ٹیکس۔'),
                    ('ED_ON_FPA',     'فیول پرائس ایڈجسٹمنٹ کی رقم پر لگنے والی الیکٹرسٹی ڈیوٹی۔'),
                    ('OTHER_FPA_TAX', 'فیول پرائس ایڈجسٹمنٹ پر لگنے والے دیگر چھوٹے ٹیکس۔'),
                    ('LP_SURCHARGE',  'مقررہ تاریخ کے بعد بل جمع کرانے پر لگنے والا جرمانہ۔'),
                    ('ARREARS',       'پچھلے بلوں کی بقایا رقم — یہ نیا استعمال یا ٹیکس نہیں ہے۔'),
                    ('DEFERRED',      'منظور شدہ قسط منصوبے کے تحت آئندہ بل پر منتقل کی گئی رقم۔')
                ) AS v(code, ur)
                WHERE c.code = v.code;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE charge_definitions SET explanation_ur = NULL;");
        }
    }
}
